using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Lamar;
using Lamar.IoC.Instances;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Montage.Weiss.Tools.Impls.PostProcessors
{
    /// <summary>
    /// Applies post-processing by searching in yuyutei for images of the cards and inserting them in.
    /// </summary>
    public class YuyuteiPostProcessor : ICardPostProcessor, ISkippable<IParseInfo>
    {
        private readonly ILogger Log = Serilog.Log.ForContext<YuyuteiPostProcessor>();

        public int Priority => 0;

        private static readonly string rarityClassPrefix = "rarity_";
        private static readonly string cardUnitListItemSelector = "#main .card_unit";
        private static readonly string cardUnitImageSelector = ".image_box > a > .image > img";
        private static readonly string cardUnitSerialSelector = ".headline > p.id";

        // Exceptional Sets
        private static string[] gfbException = new[] { "W33", "W38" };

        // Dependencies
        private readonly Func<CardDatabaseContext> _database;

        public YuyuteiPostProcessor(IContainer container)
        {
            _database = () => container.GetInstance<CardDatabaseContext>();
        }

        public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
        {
            await Task.CompletedTask;
            if (cards.First().Language != CardLanguage.Japanese)
                return false;
            var list = cards.Select(c => c.ReleaseID).Distinct().ToList();
            if (IsExceptional(list))
            {
                return true;
            }
            else if (list.Count > 1)
            {
                Log.Warning("Yuyutei Image Post-Processor is disabled for sets with multiple Release IDs; please add those images manually when prompted.");
                return false;
            }
            else
                return true;
        }

        private bool IsExceptional(List<string> releaseIDList)
        {
            if (releaseIDList.OrderBy(s => s).SequenceEqual(gfbException))
            {
                Log.Information("This is normally not allowed, but GFB vol. 2 has cards from GFB vol. 1, so YYT should allow this.");
                return true;
            }
            else
                return false;
        }

        public async Task<bool> IsIncluded(IParseInfo info)
        {
            await Task.CompletedTask;
            if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:yyt"))
            {
                Log.Information("Skipping due to the parser hint [skip:yyt].");
                return false;
            }
            else
            {
                return true;
            }
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            var yuyuteiSellPage = "https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=";

            var cards = await originalCards.ToArrayAsync();

            var firstCard = cards.Skip(Math.Min(cards.Length - 1, 10)).First(); // wtf GFB. Why you do this to me.
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

            IDocument yuyuteiSearchPage = await new Uri(yuyuteiSellPage).DownloadHTML(("Referer", "https://yuyu-tei.jp/")).WithRetries(10);

            var cardUnitListItems = yuyuteiSearchPage.QuerySelectorAll(cardUnitListItemSelector);

            var serialImageTriplets = cardUnitListItems
                .Select(div => this.CreateTrio(div))
                .Select(trio => this.Serialize(trio))
                .Distinct(trio => trio.Serial + " " + trio.Rarity)             // Dev Notes: https://yuyu-tei.jp/game_ws/sell/sell_price.php?name=BD%2fW54 gave me cancer.
                .ToDictionary(trio => (trio.Serial, trio.Rarity), pair => pair.ImageUri)
                ;

            Log.Information("Getting all PRs on card database without a YYT image link...");
            using (var db = _database())
            {
                var prCards = db.WeissSchwarzCards.AsAsyncEnumerable()
                    .Where(c =>     c.ReleaseID == setCode 
                                &&  c.Language == lang 
                                &&  c.Rarity == "PR" 
                                && !c.Images.Any(u => u.Authority == "yuyu-tei.jp")
                    );
                await foreach (var prCard in prCards)
                {
                    if (serialImageTriplets.TryGetValue((prCard.Serial, prCard.Rarity), out var urlLink))
                    {
                        var imgUrl = new Uri(urlLink);
                        prCard.Images.Add(imgUrl);
                        db.Update(prCard);
                        Log.Information("Attached to {serial}: {imgUrl}", prCard.Serial, urlLink);
                    }
                }
                await db.SaveChangesAsync();
            }

            foreach (var originalCard in cards)
            {
                var res = originalCard.Clone();
                if (serialImageTriplets.TryGetValue( (res.Serial, res.Rarity), out var urlLink)) {
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

        private (IElement serialDiv, IHtmlImageElement imageDiv, string rarityClass) CreateTrio(IElement div)
        {
            return (serialDiv: div.QuerySelector(cardUnitSerialSelector),
                        imageDiv: div.QuerySelector<IHtmlImageElement>(cardUnitImageSelector),
                        rarityClass: div.ClassList.Where(s => s.StartsWith(rarityClassPrefix)).Select(s => s.Substring(rarityClassPrefix.Length)).First()
                   );
            //return FixExceptionalYuyuteiTrios(initialResult);
        }

        private (string Serial, string Rarity, string ImageUri) Serialize((IElement serialDiv, IHtmlImageElement imageDiv, string rarityClass) trio)
        {
            var res = ( Serial: trio.serialDiv.InnerHtml.Trim(),
                        Rarity: trio.rarityClass,
                        ImageUri: trio.imageDiv.Source.Replace("ws/90_126", "ws/front")
                        );
            return res switch {
                // Fix Exceptional Serial on GU/57 caused by the serial being the same serial in SEC and in normal rarity.
                var tup when tup.Rarity == "SEC" && tup.Serial.StartsWith("GU/W57") => (tup.Serial + tup.Rarity, tup.Rarity, tup.ImageUri),
                // Fix Exceptional CC rarity when it's supposed to be a regular C for all Extra Boosters
                var tup when (tup.Serial.Contains("/WE") || tup.Serial.Contains("/SE")) 
                           && (tup.Rarity.EndsWith("CC") || tup.Rarity.EndsWith("CU"))
                  => (tup.Serial, tup.Rarity.Substring(0, tup.Rarity.Length - 2) + tup.Rarity.Last(), tup.ImageUri),
                _ => res
            };
        }
    }
}
