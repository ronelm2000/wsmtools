using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Flurl.Http;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards
{
    /// <summary>
    /// Parses results from the English site. This is done using an exploit on cardsearch that allows more than 100 cards as a single query.
    /// This being an exploit means that at some time in the future this won't work.
    /// </summary>
    public class EnglishWSURLParser : ICardSetParser
    {
        ILogger Log = Serilog.Log.ForContext<EnglishWSURLParser>();

        private static readonly string _CARD_NO_QUERY = "?cardno=";
        private static readonly string _WS_SEARCH_PAGE = "https://en.ws-tcg.com/cardlist/cardsearch/";
        private static readonly string _WS_CARD_PAGE = "https://en.ws-tcg.com/cardlist/list/?cardno=";
        private static readonly string _CARD_UNIT_SELECTOR = "table#searchResult-table td";
        private static readonly string _CARD_NAME_SELECTOR = "#searchResult-table > tbody > tr > td > h4 > a > span:nth-child(1)";
        private static readonly string _CARD_EFFECT_SELECTOR = "span:last-child";
        private static readonly Regex _UNIT_MATCHER = new Regex(@"([\[])([^\]]+)([\]]):(.+)");
        private static readonly Regex _SOUL_MATCHER = new Regex(@"partimages/soul\.gif");

        /// <summary>
        /// These are sets that use the old format. The newest sets are separated by br tags which make them easy to split, while the old ones (this list) do not.
        /// </summary>
        private static readonly string[] _SETS_USING_OLD_FORMAT = new[]
        {
            "W08", "W31", "EN-W01"
        };

        private static (string LookupString, CardColor Color)[] _COLOR_MAP = new[]
            {
                ("partimages/yellow.gif", CardColor.Yellow),
                ("partimages/green.gif", CardColor.Green),
                ("partimages/red.gif", CardColor.Red),
                ("partimages/blue.gif", CardColor.Blue)
                // Purple is unsupported by ENWS
            };

        private static Dictionary<string, CardColor> _COLOR_EXCEPTION_MAP = new Dictionary<string, CardColor>
        {
            // https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-PE02
            ["赤"] = CardColor.Red 
        };

        private static (string LookupString, Trigger CardTrigger)[] _TRIGGER_MAP = new[]
        {
            ("partimages/soul.gif", Trigger.Soul),
            ("partimages/bounce.gif", Trigger.Bounce),
            ("partimages/shot.gif", Trigger.Shot),
            ("partimages/choice.gif", Trigger.Choice),
            ("partimages/treasure.gif", Trigger.GoldBar),
            ("partimages/stock.gif", Trigger.Bag),
            ("partimages/standby.gif", Trigger.Standby),
            ("partimages/comeback.gif", Trigger.Door),
            ("partimages/gate.gif", Trigger.Gate),
            ("partimages/draw.gif", Trigger.Book)
        };

        private static (string LookupString, string Replacement)[] _EFFECT_REPLACEMENT_MAP = new[]
        {
            // https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E008 and https://en.ws-tcg.com/cardlist/list/?cardno=BD/W63-E003
            ("<img src=\"../partimages/soul.gif\">", "【SOUL】"),
            // https://en.ws-tcg.com/cardlist/list/?cardno=SY/W08-E086
            ("Draw@", "【DRAW】"),                 
            // https://en.ws-tcg.com/cardlist/list/?cardno=SY/W08-E004
            ("<img src=\"../partimages/bounce.gif\">", "【BOUNCE】") 
        };

        public bool IsCompatible(IParseInfo info)
        {
            var urlOrFile = info.URI;
            try
            {
                if (!Uri.TryCreate(urlOrFile, UriKind.Absolute, out var uri))
                {
                    Log.Debug("Not a URI. Failed compatibility check.", urlOrFile);
                    return false;
                }
                else if (uri.Authority != "en.ws-tcg.com")
                {
                    Log.Debug("The site is not EN WSTCG website. Failed compatibility check.");
                    return false;
                }
                else if (uri.AbsolutePath != "/cardlist/list/")
                {
                    Log.Debug("The site is not based on the cardlist. Failed compatibility check.");
                    return false;
                }
                else if (!uri.Query.StartsWith("?cardno="))
                {
                    Log.Debug("The site is not based on the cardlist (not having a compatible query). Failed compatibility check.");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unspecified Error during compatibility checking.");
                Log.Debug(e.Message);
                return false;
            }
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrFile)
        {
            Log.Information("Starting...");

            var uri = new Uri(urlOrFile);
            using (var fc = new FlurlClient().EnableCookies())
            {
                fc.Settings.BeforeCall = this.Debug;
                await _WS_SEARCH_PAGE.WithClient(fc).WithHTMLHeaders().GetAsync(); // To get some initial cookies.
                var serial = uri.Query.Substring(_CARD_NO_QUERY.Length);
                var serialID = WeissSchwarzCard.ParseSerial(serial);
                var wsSearchPage = await _WS_SEARCH_PAGE
                    .WithClient(fc)
                    .WithHTMLHeaders()
                    .WithHeader("Referer", _WS_SEARCH_PAGE)
                    .PostUrlEncodedAsync(new Dictionary<string,object>{
                        ["cmd"] = "search",
                        ["data[CardSearch][keyword]"] = serialID.ReleaseID,
                        ["data[CardSearch][keyword_or]"] = "",
                        ["data[CardSearch][keyword_not]"] = "",
                        ["data[CardSearch][keyword_cardname]"] = "0",
                        ["data[CardSearch][keyword_feature]"] = "0",
                        ["data[CardSearch][keyword_text]"] = "0",
                        ["data[CardSearch][keyword_cardnumber]"] = new[] { 0, 1 },
                        ["data[CardSearch][expansion]"] = "",
                        ["data[CardSearch][card_kind]"] = "",
                        ["data[CardSearch][level_s]"] = "",
                        ["data[CardSearch][level_e]"] = "",
                        ["data[CardSearch][color]"] = "",
                        ["data[CardSearch][soul_s]"] = "",
                        ["data[CardSearch][soul_e]"] = "",
                        ["data[CardSearch][cost_s]"] = "",
                        ["data[CardSearch][cost_e]"] = "",
                        ["data[CardSearch][trigger]"] = "",
                        ["data[CardSearch][option_counter]"] = "0",
                        ["data[CardSearch][option_clock]"] = "0",
                        ["data[CardSearch][show_page_count]"] = "500",
                        ["data[CardSearch][show_small]"] = "0",
                        ["button"] = "search"
                    })
                    .RecieveHTML();

                foreach (var cardUnitTD in wsSearchPage.QuerySelectorAll(_CARD_UNIT_SELECTOR))
                    yield return await ParseCardAsync(cardUnitTD);

                //Log.Information("Debug: {content}", wsSearchPage.DocumentElement.OuterHtml);
            }

            Log.Information("Ending...");
            yield break;
        }

        private async Task<WeissSchwarzCard> ParseCardAsync(IElement cardUnitTD)
        {
            WeissSchwarzCard res = new WeissSchwarzCard();

            var href = cardUnitTD.QuerySelector<IHtmlAnchorElement>("h4 a").Href;
            var name = cardUnitTD.QuerySelector(_CARD_NAME_SELECTOR).InnerHtml;
            Log.Debug("HREF: {href}", href);
            res.Serial = href.Substring(_WS_CARD_PAGE.Length);
            res.Name = new MultiLanguageString() { EN = name, JP = "" };
            res.Effect = CleanupEffect(cardUnitTD);
            foreach (var span in cardUnitTD.QuerySelectorAll(".unit"))
                await AssignUnitAsync(res, span);
            res.Remarks = $"Extractor: {this.GetType().Name}";
            return res;
        }

        private async Task AssignUnitAsync(WeissSchwarzCard card, IElement span)
        {
            var innerHTML = span.InnerHtml;
            var groups = _UNIT_MATCHER.Match(innerHTML).Groups;
            var key = groups[2].Value.ToLower();
            var value = groups[4].Value;
            Log.Debug("Key: {k} / Value: {v}", key, value);
            switch (key)
            {
                case "side":
                    card.Side = TranslateToSide(value);//  <img src=\"../partimages/s.gif\">"
                    break;
                case "card type":
                    card.Type = value.Trim().ToEnum<CardType>().Value;
                    break;
                case "level":
                    card.Level = value.Trim().AsParsed<int>(int.TryParse);
                    break;
                case "color":
                    card.Color = TranslateToColor(value); //  <img src=\"../partimages/blue.gif\">
                    break;
                case "power":
                    card.Power = value.Trim().AsParsed<int>(int.TryParse);
                    break;
                case "soul":
                    card.Soul = CountSouls(value);
                    break;
                case "cost":
                    card.Cost = value.Trim().AsParsed<int>(int.TryParse);
                    break;
                case "rarity":
                    card.Rarity = value.Trim();
                    break;
                case "trigger":
                    card.Triggers = await TranslateToTriggers(value);
                    break;
                case "special attribute":
                    card.Traits = await TranslateToTraitsAsync(value);
                    break;
                case "flavor text":
                    card.Flavor = await CleanupFlavorText(value);
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        private string[] CleanupEffect(IElement cardUnitTD)
        {
            return cardUnitTD.QuerySelectorAll(_CARD_EFFECT_SELECTOR).Last().InnerHtml
                .Split(new string[] { "<br>", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => CleanupEffect(e))
                .ToArray();
        }

        private string CleanupEffect(string effectText)
        {
            var res = effectText;
            foreach (var textPair in _EFFECT_REPLACEMENT_MAP)
                res = res.Replace(textPair.LookupString, textPair.Replacement);
            return res;
        }

        private async Task<string> CleanupFlavorText(string value)
        {
            var doc = await value.ParseHTML();
            return doc.Body.Children[0].GetInnerText();//.InnerHtml;
        }

        private async Task<List<MultiLanguageString>> TranslateToTraitsAsync(string value)
        {
            var traitInnerHTML = (await value.ParseHTML()).Body.Children[0].InnerHtml;
            return traitInnerHTML.Split("・").Select(s => new MultiLanguageString() { EN = s, JP = "" }).ToList();
        }

        private async Task<Trigger[]> TranslateToTriggers(string value)
        {
            var doc = await value.ParseHTML();
            return doc.QuerySelectorAll<IHtmlImageElement>("img")
                .SelectMany(e => _TRIGGER_MAP.Where(t => e.Source.Contains(t.LookupString)))
                .Select(t => t.CardTrigger)
                .ToArray();
        }

        private int? CountSouls(string value)
        {
            return _SOUL_MATCHER.Matches(value).Count;
        }

        private CardColor TranslateToColor(string value)
        {
            foreach (var colorLookupPair in _COLOR_MAP)
                if (value.Contains(colorLookupPair.LookupString)) return colorLookupPair.Color;

            if (_COLOR_EXCEPTION_MAP.TryGetValue(value.Trim(), out var entry))
                return entry;

            throw new NotImplementedException();
        }

        private CardSide TranslateToSide(string value)
        {
            var hasWeiss = value.Contains("partimages/w.gif");
            var hasSchwarz = value.Contains("partimages/s.gif");
            if (hasWeiss && hasSchwarz)
                return CardSide.Both;
            else if (hasWeiss)
                return CardSide.Weiss;
            else if (hasSchwarz)
                return CardSide.Schwarz;
            else
                throw new NotImplementedException("Current value is invalid: " + value);
        }

        private void Debug(HttpCall call)
        {
            //Log.Debug(call.RequestBody);
        }
    }
}
