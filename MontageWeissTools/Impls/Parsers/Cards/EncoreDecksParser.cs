using Flurl.Http;
using Montage.Weiss.Tools.API;
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
    public class EncoreDecksParser : ICardSetParser
    {
        private readonly Regex encoreDecksAPIMatcher = new Regex(@"https:\/\/www\.encoredecks\.com\/api\/series\/(.+)\/cards");
        private readonly ILogger Log = Serilog.Log.ForContext<EncoreDecksParser>();

        public bool IsCompatible(string urlOrFile)
        {
            Log.Information("Is Compatible? {urlOrFile}", urlOrFile);
             if (encoreDecksAPIMatcher.IsMatch(urlOrFile))
            {
                Log.Information("Yes");
                return true;
            }
            else
            {
                Log.Information("No");
                return false;
            }
            //            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrFile)
        {
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
                result.Name.EN = DynamicExtensions.AsOptional(setCard.locale.EN).name;
                result.Name.JP = DynamicExtensions.AsOptional(setCard.locale.NP).name;
                (List<object>, List<object>) attributes = (setCard.locale.EN.attributes, setCard.locale.NP.attributes);
                result.Traits = TranslateTraits(attributes).ToList();
                result.Effect = ((List<object>)DynamicExtensions.AsOptional(setCard)?.ability)?.Cast<string>().ToArray();
                result.Rarity = setCard.rarity;
                result.Side = TranslateSide(setCard.side);
                result.Level = (int?) setCard.level;
                result.Cost = (int?) setCard.cost;
                result.Power = (int?) setCard.power;    
                result.Soul = (int?) setCard.soul;

                result.Serial = WeissSchwarzCard.GetSerial(setCard.set.ToString(), setCard.side.ToString(), setCard.lang.ToString(), setCard.release.ToString(), setCard.sid.ToString());

                /*

                setCard.set.ToString();
            if (setCard.lang == "EN" && !((string)setCard.release.ToString()).Contains("E"))
            {
                // This is a DX set; make serial adjustments.
                fullSetID += "/EN-" + setCard.side.ToString();
            } else
            {
                // Proceed as normal
                fullSetID += "/" + setCard.side.ToString();
            }
            fullSetID += setCard.release.ToString();
            result.Serial = fullSetID + "-" + setCard.sid.ToString();

*/


                result.Type = TranslateType(setCard.cardtype);
                result.Color = TranslateColor(setCard.colour);
                result.Remarks = $"Parsed: {this.GetType().Name}";  
                yield return result;
            }
            // Get 
            yield break;
        }



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
            var maxlength = Math.Max(attributes.EN.Count, attributes.JP.Count);
            var enSpan = attributes.EN;
            var jpSpan = attributes.JP;
            for (int i = 0; i < maxlength; i++)
            {
                MultiLanguageString str = new MultiLanguageString();
                str.EN = (i < enSpan.Count) ? enSpan[i].ToString() : null;
                str.JP = (i < jpSpan.Count) ? jpSpan[i].ToString() : null;
                yield return str;
            }
        }
    }
}
