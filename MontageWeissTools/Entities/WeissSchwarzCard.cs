using Flurl.Http;
using Montage.Card.API.Utilities;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Weiss.Tools.Utilities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;
using Octokit;
using Fluent.IO;

namespace Montage.Weiss.Tools.Entities;

public class WeissSchwarzCard : IExactCloneable<WeissSchwarzCard>, ICard
{
    private static ILogger Log = Serilog.Log.ForContext<WeissSchwarzCard>();

    private static string[] foilRarities = new[] { "SR", "SSR", "RRR", "SPM", "SPa", "SPb", "SP", "SSP", "SEC", "XR", "BDR" };
    private static string[] englishEditedPrefixes = new[] { "EN-", "S25", "W30" };
    private static string[] englishOriginalPrefixes = new[] { "Wx", "SX", "BSF", "BCS" };
    private static string[] customPrefixes = new[] { "WC", "SC", "WSC" };

    public static IEqualityComparer<WeissSchwarzCard> SerialComparer { get; internal set; } = new WeissSchwarzCardSerialComparerImpl();
    public static readonly WeissSchwarzCard Empty = new WeissSchwarzCard();

    public string Serial { get; set; }
    public MultiLanguageString Name { get; set; }
    public List<WeissSchwarzTrait> Traits { get; set; } = new List<WeissSchwarzTrait>();
    public CardType Type { get; set; }
    public CardColor Color { get; set; }
    public CardSide Side { get; set; }
    public string Rarity { get; set; }

    public int? Level { get; set; }
    public int? Cost { get; set; }
    public int? Soul { get; set; }
    public int? Power { get; set; }
    public Trigger[] Triggers { get; set; }
    public string? Flavor { get; set; }
    public string[] Effect { get; set; }
    public List<Uri> Images { get; set; } = new List<Uri>();
    public string VersionTimestamp { get; set; }
    public string Remarks { get; set; }

    public virtual List<WeissSchwarzCardOptionalInfo> AdditionalInfo { get; set; } = new List<WeissSchwarzCardOptionalInfo>();

    /// <summary>
    /// File Path Relative Link into a cached image. This property is usually assigned exactly once by
    /// <see cref="IExportedDeckInspector">Deck Inspectors</see>
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public string? CachedImagePath { get; set; }
    
    public WeissSchwarzCard()
    {
        Log ??= Serilog.Log.ForContext<WeissSchwarzCard>();
        Effect = Array.Empty<string>();
        Name = MultiLanguageString.Empty;
        Rarity = string.Empty;
        Triggers = Array.Empty<Trigger>();
        Serial = string.Empty;
        VersionTimestamp = Program.AppVersion;
        Remarks = string.Empty;
    }

    /// <summary>
    /// Gets the Full Release ID
    /// </summary>
    public string ReleaseID => ParseRID(Serial); // Serial.AsSpan().Slice(s => s.IndexOf('/') + 1); s => s.IndexOf('-')).ToString();
    public CardLanguage Language => GetLanguage(Serial);
    public EnglishSetType? EnglishSetType => GetEnglishSetType(Language, ReleaseID);
    public bool IsFoil => foilRarities.Contains(Rarity);

    public string TitleCode => ParseSerial(Serial).NeoStandardCode;

    //public bool IsEnglishEdited => GetIsEnglishEdited(Language, ReleaseID);
    //public bool IsEnglishOriginal => ReleaseID is string rid && englishOriginalPrefixes.Any(c => c.StartsWith(rid));

    public WeissSchwarzCard Clone()
    {
        WeissSchwarzCard newCard = (WeissSchwarzCard) this.MemberwiseClone();
        newCard.Name = new MultiLanguageString { EN = this.Name.EN, JP = this.Name.JP };
        newCard.Traits = this.Traits.Select(s => s.Clone()).ToList();
        newCard.Images = this.Images.ToList();
        return newCard;
    }

    public Path? GetCachedImagePath(String imagePath = "Images")
    {
        return Path.Get(AppDomain.CurrentDomain.BaseDirectory)
                        .Add("Images")
                        .Files($"{Serial.Replace('-', '_').AsFileNameFriendly()}.*", true)
                        .WhereExtensionIs(".png", ".jpeg", ".jpg", "jfif")
                        .FirstOrDefault();
    }

    public async Task<System.IO.Stream> GetImageStreamAsync(CookieSession? cookieSession, CancellationToken ct)
    {
        if (!String.IsNullOrWhiteSpace(CachedImagePath) && !CachedImagePath.Contains(".."))
            try
            {
                if (System.IO.File.Exists(CachedImagePath))
                    return System.IO.File.OpenRead(CachedImagePath);
                else
                {
                    Log.Warning("Cannot find cache file: {cacheImagePath}.", CachedImagePath);
                    Log.Warning("Falling back on remote URL.");
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Log.Warning("Cannot find cache file: {cacheImagePath}.", CachedImagePath);
                Log.Warning("Falling back on remote URL.");
            }
            catch (Exception) { }
        var url = Images.Last();
        Log.Debug("Loading URL: {url}", url.AbsoluteUri);

        var builder = url.WithImageHeaders();
        if (cookieSession is not null)
            builder = builder.WithCookies(cookieSession!);
        
        return await builder.GetAsync(cancellationToken: ct).ReceiveStream();
    }

    public async Task<bool> IsImagePresentAsync(CookieSession? cookieSession, CancellationToken ct)
    {
        if (!String.IsNullOrWhiteSpace(CachedImagePath) && !CachedImagePath.Contains(".."))
            try
            {
                if (System.IO.File.Exists(CachedImagePath))
                    return true;
            }
            catch (Exception) { }
        
        var url = Images.Last();
        try
        {
            return (await url.WithImageHeaders()
                            .WithCookies(cookieSession)
                            .GetAsync(cancellationToken: ct))
                            .StatusCode == 200;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsExceptionalSerial(string serial)
    {
        var (NeoStandardCode, ReleaseID, SetID) = ParseSerial(serial);
        if (ReleaseID == "W02" && SetID.StartsWith("E")) return true; // https://heartofthecards.com/code/cardlist.html?pagetype=ws&cardset=wslbexeb is an exceptional serial.
        else return false;
    }

    public static EnglishSetType? GetEnglishSetType(string serial)
    {
        return GetEnglishSetType(GetLanguage(serial), ParseRID(serial));
    }

    private static EnglishSetType? GetEnglishSetType(CardLanguage language, string releaseID)
    {
        if (language != CardLanguage.English) return null;
        else if (englishEditedPrefixes.Any(prefix => releaseID.StartsWith(prefix))) return Entities.EnglishSetType.EnglishEdition;
        else if (englishOriginalPrefixes.Any(prefix => releaseID.StartsWith(prefix))) return Entities.EnglishSetType.EnglishOriginal;
        else if (customPrefixes.Any(prefix => releaseID.StartsWith(prefix))) return Entities.EnglishSetType.Custom;
        else return Entities.EnglishSetType.JapaneseImport;
    }

    /// <summary>
    /// Infers the Language of a Weiss Schwarz valid serial.
    /// </summary>
    /// <param name="serial"></param>
    /// <returns></returns>
    internal static CardLanguage GetLanguage(string serial)
    {
        if (serial.Contains("-E"))
        {
            if (!IsExceptionalSerial(serial)) return CardLanguage.English;
            else return CardLanguage.Japanese;
        }
        else if (serial.Contains("-PE")) return CardLanguage.English;
        else if (serial.Contains("-TE")) return CardLanguage.English;
        else if (serial.Contains("/WX")) return CardLanguage.English;
        else if (serial.Contains("/SX")) return CardLanguage.English;
        else if (serial.Contains("/EN-")) return CardLanguage.English;
        else if (serial.Contains("/BSF")) return CardLanguage.English; // BSF is the English version of WCS for Spring
        else if (serial.Contains("/BCS")) return CardLanguage.English; // BCS is the English version of WCS for Winter
        else return CardLanguage.Japanese;
    }

    /// <summary>
    /// Returns a new serial which is the non-foil version.
    /// </summary>
    /// <param name="serial"></param>
    /// <returns></returns>
    public static string RemoveFoil(string serial)
    {
        var parsedSerial = ParseSerial(serial);
        var regex = new Regex(@"([A-Z]*)([0-9]+)([a-z]*)([a-zA-Z]*)");
        if (regex.Match(parsedSerial.SetID) is Match m) parsedSerial.SetID = $"{m.Groups[1]}{m.Groups[2]}{m.Groups[3]}";
        return parsedSerial.AsString();
    }

    internal static string AsJapaneseSerial(string serial)
    {
        var lang = GetLanguage(serial);
        var serialTuple = ParseSerial(serial);
        if (GetEnglishSetType(lang, serialTuple.ReleaseID) != Entities.EnglishSetType.JapaneseImport) return serial;
        var regex = new Regex(@"(P|T)?(E)(.+)");
        var match = regex.Match(serialTuple.SetID);
        serialTuple.SetID = $"{match.Groups[1]}{match.Groups[3]}";
        return serialTuple.AsString();
    }

    internal static string AsEnglishSerial(string serial)
    {
        var lang = GetLanguage(serial);
        var serialTuple = ParseSerial(serial);
        if (GetLanguage(serial) != CardLanguage.Japanese) return serial;
        var regex = new Regex(@"(P|T)?(.+)");
        var match = regex.Match(serialTuple.SetID);
        serialTuple.SetID = $"{match.Groups[1]}E{match.Groups[2]}";
        return serialTuple.AsString();
    }

    public static SerialTuple ParseSerial(string serial)
    {
        try
        {
            SerialTuple res = new SerialTuple();
            res.NeoStandardCode = serial.Substring(0, serial.IndexOf('/'));
            var slice = serial.AsSpan().Slice(serial.IndexOf('/'));
            res.ReleaseID = ParseRID(serial);
            slice = slice.Slice(res.ReleaseID.Length + 2);
            res.SetID = slice.ToString();
            //res.
            return res;
        } catch (ArgumentOutOfRangeException)
        {
            return default;
        }
    }

    private static string ParseRID(string serial)
    {
        var span = serial.AsSpan().Slice(s => s.IndexOf('/') + 1);
        var endAdjustment = (span.StartsWith("EN")) ? 3 : 0;
        return span.Slice(0, span.Slice(endAdjustment).IndexOf('-') + endAdjustment).ToString();
    }

    public static string GetSerial(string subset, string side, string lang, string releaseID, string setID)
    {
        string fullSetID = subset;
        var setFormatter = GetExceptionalSetFormat(lang, side + releaseID);
        var cardFormatter = GetExceptionalCardFormat(lang, releaseID, setID);
        if (setFormatter is not null)
            return setFormatter((subset, side, lang, releaseID, setID));
        else if (cardFormatter is not null)
            return cardFormatter((subset, side, lang, releaseID, setID));
        else if (lang == "EN" && !setID.Contains("E") && !releaseID.StartsWith("X"))
            return $"{subset}/EN-{side}{releaseID}-{setID}"; // This is a DX set serial adjustment.
        else
            return $"{subset}/{side}{releaseID}-{setID}";
    }

    private static Func<(string subset, string side, string lang, string releaseID, string setID), string>? GetExceptionalSetFormat(string lang, string fullReleaseID)
    {
        return (lang, fullReleaseID) switch {
            ("EN", "S04") => (tuple) => $"{tuple.subset}/EN-{tuple.side}{tuple.releaseID}-{tuple.setID}",
            _ => null
        };
    }

    private static Func<(string subset, string side, string lang, string releaseID, string setID), string>? GetExceptionalCardFormat(string lang, string releaseID, string setID)
    {
        return (lang, releaseID, setID) switch
        {
            ("EN", "X01", "X02") => (tuple) => "BNJ/BCS2019-02",
            var tuple when tuple.lang == "EN" && tuple.setID.Contains("-") => (tuple) => $"{tuple.subset}/{tuple.setID}",
            _ => null
        };
    }

    public string TypeToString(){
        string res = "";
        switch(this.Type){
            case CardType.Character:
                res = "CH";
                break; 
            case CardType.Event:
                res = "EV";
                break; 
            case CardType.Climax:
                res = "CX";
                break; 
        }
        return res;
    }

    internal WeissSchwarzCard WithTrait(WeissSchwarzTrait[] weissSchwarzTraits)
    {
        this.Traits.RemoveAll(i => true);
        this.Traits.AddRange(weissSchwarzTraits);
        return this;
    }

    public T? FindOptionalInfo<T>(string key)
    {
        if (string.IsNullOrEmpty(key)) return default;
        var info = AdditionalInfo.FirstOrDefault(i => i?.Key == key, null);
        if (info is null) return default;
        return info.DeserializeValue<T>();
    }

    public void AddOptionalInfo<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (value is null) return;
        var json = JsonSerializer.Serialize<T>(value);
        if (string.IsNullOrEmpty(json)) return;
        var info = AdditionalInfo.FirstOrDefault(i => i.Key == key, new WeissSchwarzCardOptionalInfo(this, key));
        info.Card = this;
        info.SerializeValue<T>(value);

        AdditionalInfo.Remove(info);
        AdditionalInfo.Add(info);
    }

}

internal class WeissSchwarzCardSerialComparerImpl : IEqualityComparer<WeissSchwarzCard>
{
    public bool Equals([AllowNull] WeissSchwarzCard x, [AllowNull] WeissSchwarzCard y)
        => x?.Serial == y?.Serial;

    public int GetHashCode([DisallowNull] WeissSchwarzCard obj)
    {
        return obj.Serial.GetHashCode();
    }
}

public struct SerialTuple
{
    public string NeoStandardCode;
    public string ReleaseID;
    public string SetID;

    public void Deconstruct(out string NeoStandardCode, out string ReleaseID, out string SetID)
    {
        NeoStandardCode = this.NeoStandardCode;
        ReleaseID = this.ReleaseID;
        SetID = this.SetID;
    }

    internal string AsString()
    {
        return $"{NeoStandardCode}/{ReleaseID}-{SetID}";
    }
}

public static class CardEnumExtensions
{
    public static T? ToEnum<T>(this ReadOnlySpan<char> stringSpan) where T : struct, System.Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        foreach (var e in values)
            if (stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase))
                return e;
        return null;
        //return values.Where(e => stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase)).First();
    }

    public static T? ToEnum<T>(this string stringSpan) where T : struct, System.Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        foreach (var e in values)
            if (stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase))
                return e;
        return null;
        //return values.Where(e => stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase)).First();
    }

    public static string AsShortString(this CardType cardType) => cardType switch
    {
        CardType.Character => "CH",
        CardType.Event => "EV",
        CardType.Climax => "CX",
        var str => throw new Exception($"Cannot parse {typeof(CardType).Name} from {str}")
    };

    public static int GetSortKey(this CardType cardType) => cardType switch
    {
        CardType.Character => 0,
        CardType.Event => 1,
        CardType.Climax => 2,
        _ => 3
    };

    public static int GetSortKey(this CardColor cardColor) => cardColor switch
    {
        CardColor.Yellow => 0,
        CardColor.Green => 1,
        CardColor.Red => 2,
        CardColor.Blue => 3,
        CardColor.Purple => 4,
        _ => 5
    };
}

public enum EnglishSetType
{
    JapaneseImport,
    EnglishEdition,
    EnglishOriginal,
    Custom
}

public enum CardType
{
    Character,
    Event,
    Climax
}


public enum CardColor
{
    Yellow,
    Green,
    Red,
    Blue,
    Purple
}

public enum Trigger
{
    Soul,
    Shot,
    Bounce,
    Choice,
    GoldBar,
    Bag,
    Door,
    Standby,
    Book,
    Gate
}

public enum CardSide
{
    Weiss,
    Schwarz,
    Both
}

public enum CardLanguage
{
    English,
    Japanese
}
