using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards
{
    /// <summary>
    /// Parses card set results from the EncoreDecks API. Thanks to the dev who made this API for consumption.
    /// </summary>
    public class EncoreDecksParser : ICardSetParser<WeissSchwarzCard>
    {
        private readonly Regex encoreDecksAPIMatcher = new Regex(@"http(?:s)?:\/\/www\.encoredecks\.com\/api\/series\/(.+)\/cards");
        private readonly Regex encoreDecksSiteSetMatcher = new Regex(@"http(?:s)?:\/\/www.encoredecks\.com\/?.+&set=([a-f0-9]+).*");
        private readonly ILogger Log = Serilog.Log.ForContext<EncoreDecksParser>();

        public bool IsCompatible(IParseInfo info)
        {
            var urlOrFile = info.URI;
            if (encoreDecksAPIMatcher.IsMatch(urlOrFile))
            {
                Log.Information("Compatibility Passed for: {urlOrFile}", urlOrFile);
                return true;
            }
            else if (encoreDecksSiteSetMatcher.IsMatch(urlOrFile))
            {
                Log.Information("Compatibility Passed for: {urlOrFile}", urlOrFile);
                return true;
            }
            else
            {
                Log.Debug("Compatibility Failed for: {urlOrFile}", urlOrFile);
                return false;
            }
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrFile)
        {
            if (encoreDecksSiteSetMatcher.IsMatch(urlOrFile))
                urlOrFile = TransformIntoAPIFormat(urlOrFile);

            IList<dynamic> setCards = null;
            do try
                {
                    setCards = await urlOrFile.WithRESTHeaders().GetJsonListAsync();
                }
                catch (FlurlHttpException)
                {
                    // Do nothing
                } while (setCards == null);
            foreach (var setCard in setCards)
            {
                WeissSchwarzCard result = new WeissSchwarzCard();
                result.Name = new MultiLanguageString();
                var enOptional = DynamicExtensions.AsOptional(setCard.locale.EN);
                var jpOptional = DynamicExtensions.AsOptional(setCard.locale.NP);
                if (((string)enOptional.source)?.ToLower() != "akiba")
                    result.Name.EN = enOptional.name;
                result.Name.JP = jpOptional.name;
                (List<object>, List<object>) attributes = (enOptional.attributes, jpOptional.attributes);
                result.Traits = TranslateTraits(attributes).ToList();
                result.Effect = ((List<object>)enOptional.ability)?.Cast<string>().ToArray();
                result.Rarity = setCard.rarity;
                result.Side = TranslateSide(setCard.side);
                result.Level = (int?) setCard.level;
                result.Cost = (int?) setCard.cost;
                result.Power = (int?) setCard.power;    
                result.Soul = (int?) setCard.soul;
                result.Triggers = TranslateTriggers(setCard.trigger);

                //result.Serial = setCard.cardcode;
                if (!String.IsNullOrEmpty(setCard.imagepath))
                    result.Images.Add(new Uri($"https://www.encoredecks.com/images/{setCard.imagepath}"));

                // TODO: Delete all methods related with generating serial.
                // TODO: Switch once LLDX checkbox is checked properly. See: https://trello.com/c/WCT94Sk0/2-card-code-needs-to-be-stored-seperatly-from-side-release
                result.Serial = WeissSchwarzCard.GetSerial(setCard.set.ToString(), setCard.side.ToString(), setCard.lang.ToString(), setCard.release.ToString(), setCard.sid.ToString());

                result.Type = TranslateType(setCard.cardtype);
                result.Color = TranslateColor(setCard.colour);
                result.Remarks = $"Parsed: {this.GetType().Name}";  
                yield return result;
            }
            // Get 
            yield break;
        }

        private string TransformIntoAPIFormat(string urlOrFile)
        {
            Log.Information("Converting URL into API link...");
            return TransformIntoAPIFormatFromSetGUID(encoreDecksSiteSetMatcher.Match(urlOrFile).Groups[1].Value);
        }

        private string TransformIntoAPIFormatFromSetGUID(string setGUID)
            => $"https://www.encoredecks.com/api/series/{setGUID}/cards";

        private CardColor TranslateColor(string color)
        {
            return color switch
            {
                "YELLOW" => CardColor.Yellow,
                "GREEN" => CardColor.Green,
                "BLUE" => CardColor.Blue,
                "RED" => CardColor.Red,
                "PURPLE" => CardColor.Purple,
                _ => throw new Exception($"Cannot parse {typeof(CardColor).Name} from {color}")
            };
         }

        private CardType TranslateType(string cardtype)
        {
            return cardtype switch
            {
                "CH" => CardType.Character,
                "EV" => CardType.Event,
                "CX" => CardType.Climax,
                _ => throw new Exception($"Cannot parse {typeof(CardType).Name} from {cardtype}")
            };
        }

        private CardSide TranslateSide(string side)
        {
            return side switch
            {
                "W" => CardSide.Weiss,
                "S" => CardSide.Schwarz,
                _ => CardSide.Both //TODO: This should be changed to the proper value for both, and the rest of the values will display an error.
            };
        }

        private IEnumerable<MultiLanguageString> TranslateTraits((List<object> EN, List<object> JP) attributes)
        {
            var nullCheckedAttributes = (EN: attributes.EN ?? Enumerable.Empty<object>().ToList(), JP: attributes.JP ?? Enumerable.Empty<object>().ToList());
            var maxlength = Math.Max(nullCheckedAttributes.EN.Count, nullCheckedAttributes.JP.Count);
            var enSpan = nullCheckedAttributes.EN.ToArray();
            var jpSpan = nullCheckedAttributes.JP.ToArray();
            var maxLength = Math.Max(enSpan.Length, jpSpan.Length);
            Array.Resize(ref enSpan, maxLength);
            Array.Resize(ref jpSpan, maxLength);
            return enSpan.Zip(jpSpan, Construct).Where(mls => mls != null);
        }

        private static readonly string[] NULL_TRAITS = new[]
        {
            "-",
            "0",
            "－"
        };

        private MultiLanguageString Construct(object traitEN, object traitJP)
        {
            MultiLanguageString str = new MultiLanguageString();
            str.EN = traitEN?.ToString();
            str.JP = traitJP?.ToString();
            str.EN = (String.IsNullOrWhiteSpace(str.EN) || NULL_TRAITS.Contains(str.EN)) ? null : str.EN;
            str.JP = (String.IsNullOrWhiteSpace(str.JP)) ? null : str.JP;
            if (str.EN == null && str.JP == null)
                return null;
            else
                return str;
        }

        private Trigger[] TranslateTriggers(List<object> triggers) => triggers.Select(o => o.ToString()).Select(TranslateTrigger).ToArray();

        private Trigger TranslateTrigger(string trigger)
        {
            return trigger.ToLower() switch
            {
                // Yellow
                "soul" => Trigger.Soul,
                "shot" => Trigger.Shot,
                "return" => Trigger.Bounce,
                "wind" => Trigger.Bounce,
                "choice" => Trigger.Choice,
                // Green
                "treasure" => Trigger.GoldBar,
                "bag" => Trigger.Bag,
                "pool" => Trigger.Bag,
                // Red
                "comeback" => Trigger.Door,
                "salvage" => Trigger.Door,
                "standby" => Trigger.Standby,
                // Blue
                "draw" => Trigger.Book,
                "gate" => Trigger.Gate,
                var str => throw new Exception($"Cannot parse {typeof(Trigger).Name} from {str}")
            };
        }
    }
}
