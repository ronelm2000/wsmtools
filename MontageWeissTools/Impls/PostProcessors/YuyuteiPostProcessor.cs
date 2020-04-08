using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Montage.Weiss.Tools.Impls.PostProcessors
{
    /// <summary>
    /// Applies post-processing by searching in yuyutei for images of the cards and inserting them in.
    /// </summary>
    public class YuyuteiPostProcessor : ICardPostProcessor
    {
        private readonly ILogger Log = Serilog.Log.ForContext<YuyuteiPostProcessor>();

        public int Priority => 0;

        public bool IsCompatible(List<WeissSchwarzCard> cards)
        {
            if (cards.First().Language != CardLanguage.Japanese)
                return false;
            else if (cards.Select(c => c.ReleaseID).Count() > 1)
            {
                Log.Warning("Yuyutei Image Post-Processor is disabled for sets with multiple Release IDs; please add those images manually when prompted.");
                return false;
            }
            else
                return true;
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            var yuyuteiSellPage = "https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=";
            var cardUnitListItemSelector = "#main .card_unit";
            var cardUnitImageSelector = ".image_box > a > .image > img";
            var cardUnitSerialSelector = ".headline > p.id";
            var firstCard = await originalCards.FirstAsync();
            var setCode = firstCard.ReleaseID;
            var lang = firstCard.Language;

            if (lang == CardLanguage.English) // Yuyutei Inquiry will just crash
            { 
                await foreach (var card in originalCards) yield return card;
                yield break;
            }
            
            Log.Information("Starting...");

            yuyuteiSellPage += HttpUtility.UrlEncode(setCode);
            Log.Information("Loading: {yuyuteiSellPage}", yuyuteiSellPage);
            IDocument yuyuteiSearchPage = await new Uri(yuyuteiSellPage).DownloadHTML( ("Referer", "https://yuyu-tei.jp/") );

            var cardUnitListItems = yuyuteiSearchPage.QuerySelectorAll(cardUnitListItemSelector);

            var serialImagePairs = cardUnitListItems
                .Select(div => (serialDiv: div.QuerySelector(cardUnitSerialSelector), imageDiv: div.QuerySelector<IHtmlImageElement>(cardUnitImageSelector)))
                .Select(pair => (Serial: pair.serialDiv.InnerHtml.Trim(), ImageUri: pair.imageDiv.Source.Replace("ws/90_126", "ws/front")))
                .Distinct(pair => pair.Serial)             // Dev Notes: https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=BD%2fW54 gave me cancer.
                .ToDictionary(pair => pair.Serial, pair => pair.ImageUri)
                ;

            await foreach (var originalCard in originalCards)
            {
                var res = originalCard.Clone();
                if (serialImagePairs.TryGetValue(res.Serial, out var urlLink)) {
                    var imgUrl = new Uri(urlLink);
                    res.Images.Add(imgUrl);
                    Log.Information("Attached to {serial}: {imgUrl}", res.Serial, urlLink);
                } else
                {
                    Log.Warning("Yuyutei did not have an image for {serial}, you should check for other image sources and add it manually.", res.Serial);
                }
                yield return res;
            }
            Log.Information("Ended.");
            yield break;
        }
    }
}
