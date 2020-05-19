using Flurl.Http;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities
{
    public class WeissSchwarzCard : IExactCloneable<WeissSchwarzCard>
    {
        private static ILogger Log;

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

        public CardLanguage Language => TranslateToLanguage();

        public static IEqualityComparer<WeissSchwarzCard> SerialComparer { get; internal set; } = new WeissSchwarzCardSerialComparerImpl();

        public WeissSchwarzCard Clone()
        {
            WeissSchwarzCard newCard = (WeissSchwarzCard) this.MemberwiseClone();
            newCard.Name = this.Name.Clone();
            newCard.Traits = this.Traits.Select(s => s.Clone()).ToList();
            return newCard;
        }

        public async Task<System.IO.Stream> GetImageStreamAsync()
        {
            int retry = 0;
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
            do try
                {
                    return await Images.Last().WithImageHeaders().GetStreamAsync();
                }
                catch (Exception e) {
                    if (retry++ > 9) throw e;
                } 
            while (true);
        }
        
        private CardLanguage TranslateToLanguage()
        {
            var serial = Serial;
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

        private static bool IsExceptionalSerial(string serial)
        {
            var (NeoStandardCode, ReleaseID, SetID) = ParseSerial(serial);
            if (ReleaseID == "W02" && SetID.StartsWith("E")) return true; // https://heartofthecards.com/code/cardlist.html?pagetype=ws&cardset=wslbexeb is an exceptional serial.
            else return false;
            throw new NotImplementedException();
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
