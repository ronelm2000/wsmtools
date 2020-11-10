using Flurl.Http;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities
{
    public class WeissSchwarzCard : IExactCloneable<WeissSchwarzCard>
    {
        private static ILogger Log;
        private static string[] foilRarities = new[] { "SR", "SSR", "RRR", "SPM", "SPa", "SPb", "SP", "SSP", "SEC", "XR", "BDR" };
        private static string[] englishEditedPrefixes = new[] { "EN-", "S25", "W30" };
        private static string[] englishOriginalPrefixes = new[] { "Wx", "SX", "BSF", "BCS" };

        public static IEqualityComparer<WeissSchwarzCard> SerialComparer { get; internal set; } = new WeissSchwarzCardSerialComparerImpl();

        public string Serial { get; set; }
        public MultiLanguageString Name { get; set; }
        public List<MultiLanguageString> Traits { get; set; }
        public CardType Type { get; set; }
        public CardColor Color { get; set; }
        public CardSide Side { get; set; }
        public string Rarity { get; set; }

        public int? Level { get; set; }
        public int? Cost { get; set; }
        public int? Soul { get; set; }
        public int? Power { get; set; }
        public Trigger[] Triggers { get; set; }
        public string Flavor { get; set; }
        public string[] Effect { get; set; }
        public List<Uri> Images { get; set; } = new List<Uri>();
        public string Remarks { get; set; }
        
        /// <summary>
        /// File Path Relative Link into a cached image. This property is usually assigned exactly once by
        /// <see cref="IExportedDeckInspector">Deck Inspectors</see>
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public string CachedImagePath { get; set; }

        //public readonly WeissSchwarzCard Empty = new WeissSchwarzCard();

        public WeissSchwarzCard()
        {
            Log ??= Serilog.Log.ForContext<WeissSchwarzCard>();
        }

        /// <summary>
        /// Gets the Full Release ID
        /// </summary>
        public string ReleaseID => ParseRID(Serial); // Serial.AsSpan().Slice(s => s.IndexOf('/') + 1); s => s.IndexOf('-')).ToString();
        public CardLanguage Language => GetLanguage(Serial);
        public EnglishSetType? EnglishSetType => GetEnglishSetType(Language, ReleaseID);
        public bool IsFoil => foilRarities.Contains(Rarity);
        //public bool IsEnglishEdited => GetIsEnglishEdited(Language, ReleaseID);
        //public bool IsEnglishOriginal => ReleaseID is string rid && englishOriginalPrefixes.Any(c => c.StartsWith(rid));

        public WeissSchwarzCard Clone()
        {
            WeissSchwarzCard newCard = (WeissSchwarzCard) this.MemberwiseClone();
            newCard.Name = this.Name.Clone();
            newCard.Traits = this.Traits.Select(s => s.Clone()).ToList();
            return newCard;
        }

        public async Task<System.IO.Stream> GetImageStreamAsync()
        {
            if (!String.IsNullOrWhiteSpace(CachedImagePath) && !CachedImagePath.Contains(".."))
                try
                {
                    return System.IO.File.OpenRead(CachedImagePath);
                }
                catch (System.IO.FileNotFoundException)
                {
                    Log.Warning("Cannot find cache file: {cacheImagePath}.", CachedImagePath);
                    Log.Warning("Falling back on remote URL.");
                }
                catch (Exception) { }
            var url = Images.Last();
            Log.Debug("Loading URL: {url}", url.AbsoluteUri);

            /*
            var response = await url.WithImageHeaders()
                                .WithReferrer(url.AbsoluteUri)
                                .GetAsync();
            Log.Debug("Done, reading content: {url}", url.AbsoluteUri);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes);
            */
            
            return await url.WithImageHeaders()
                            .GetAsync()
                            .ReceiveStream();
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
        internal static string RemoveFoil(string serial)
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

        public static SerialTuple ParseSerial(string serial)
        {
            SerialTuple res = new SerialTuple();
            res.NeoStandardCode = serial.Substring(0, serial.IndexOf('/'));
            var slice = serial.AsSpan().Slice(serial.IndexOf('/'));
            res.ReleaseID = ParseRID(serial);
            slice = slice.Slice(res.ReleaseID.Length + 2);
            res.SetID = slice.ToString();
            //res.
            return res;
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
            if (TryGetExceptionalSetFormat(lang, side + releaseID, out var formatter))
            {
                return formatter((subset, side, lang, releaseID, setID));
            }
            else if (TryGetExceptionalCardFormat(lang, releaseID, setID, out var formatter2))
            {
                return formatter2((subset, side, lang, releaseID, setID));
            }
            else if (lang == "EN" && !setID.Contains("E") && !releaseID.StartsWith("X"))
            {
                return $"{subset}/EN-{side}{releaseID}-{setID}"; // This is a DX set serial adjustment.
            }
            else
            {
                return $"{subset}/{side}{releaseID}-{setID}";
            }
        }

        private static bool TryGetExceptionalSetFormat(string lang, string fullReleaseID, out Func<(string subset, string side, string lang, string releaseID, string setID), string> formatter)
        {
            formatter = (lang, fullReleaseID) switch {
                ("EN", "S04") => (tuple) => $"{tuple.subset}/EN-{tuple.side}{tuple.releaseID}-{tuple.setID}",
                _ => null
            };
            return formatter != null;
        }

        private static bool TryGetExceptionalCardFormat(string lang, string releaseID, string setID, out Func<(string subset, string side, string lang, string releaseID, string setID), string> formatter)
        {
            formatter = (lang, releaseID, setID) switch
            {
                ("EN", "X01", "X02") => (tuple) => "BNJ/BCS2019-02",
                var tuple when tuple.lang == "EN" && tuple.setID.Contains("-") => (tuple) => $"{tuple.subset}/{tuple.setID}",
                _ => null
            };
            return formatter != null;
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
    }

    internal class WeissSchwarzCardSerialComparerImpl : IEqualityComparer<WeissSchwarzCard>
    {
        public bool Equals([AllowNull] WeissSchwarzCard x, [AllowNull] WeissSchwarzCard y)
        {
            if (x == null) return y == null;
            else return x.Serial == y.Serial;
        }

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
    }

    public enum EnglishSetType
    {
        JapaneseImport,
        EnglishEdition,
        EnglishOriginal
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
}
