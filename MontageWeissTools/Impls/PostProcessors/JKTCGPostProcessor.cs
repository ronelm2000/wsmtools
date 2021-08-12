using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Lamar;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
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

namespace Montage.Weiss.Tools.Impls.PostProcessors
{
    public class JKTCGPostProcessor : ICardPostProcessor<WeissSchwarzCard>
    {
        private readonly ILogger Log = Serilog.Log.ForContext<JKTCGPostProcessor>();
        private readonly Regex LinkMatcher = new Regex(@"(http:\/\/jktcg.com\/)(EN-.+-)(.+)");
        private readonly string[] globalReleasePrefixes = { "BSF", "BCS" };

        public int Priority => 0;

        // Dependencies
        private readonly Func<CardDatabaseContext> _database;

        public JKTCGPostProcessor(IContainer container)
        {
            _database = () => container.GetInstance<CardDatabaseContext>();
        }

        public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
        {
            await Task.CompletedTask;
            var firstCard = cards.First();
            if (firstCard.Language != CardLanguage.English)
                return false;

            var allReleaseIDs = cards.Select(c => c.ReleaseID)
                .Distinct()
                .Where(rid => !rid.StartsWithAny(globalReleasePrefixes)) // Remove all global prefixes as those are assigned on the same page as the main set.
                .ToArray();

            if (allReleaseIDs.Length == 2 && allReleaseIDs.Contains("W53") && allReleaseIDs.Contains("WE27"))
            {
                Log.Information("JKTCG Image Post-Processor is normally disabled for sets with multiple Release IDs.");
                Log.Information("But the set W53 is located in WE27 for JKTCG, so this Post-Processor is deemed compatible.");
                return true;
            }
            else if (allReleaseIDs.Length > 1)
            {
                Log.Warning("JKTCG Image Post-Processor is disabled for sets with multiple Release IDs; please add those images manually when prompted.");
                return false;
            }
            else if (!(await GetSetListURI(firstCard)).HasValue)
            {
                Log.Information("Unable to find info from JKTCG; likely a new set, will skip.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            var firstCard = originalCards.FirstAsync().Result;
            var lang = firstCard.Language;
            if (lang == CardLanguage.Japanese) return originalCards;
            else return Process(firstCard, originalCards);
        }

        private async IAsyncEnumerable<WeissSchwarzCard> Process(WeissSchwarzCard firstCard, IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            Log.Information("Starting...");
            (string setLinkWithUnderscores, string url)? pair = await GetSetListURI(firstCard);
            var cardList = await pair?.url
                .WithHTMLHeaders()
                .GetHTMLAsync();
            var releaseID = firstCard.ReleaseID;
            var cardImages = cardList.QuerySelectorAll<IHtmlImageElement>("a > img")
                .Select(ele => (
                    Serial: GetSerial(ele),
                    Source: ele.GetAncestor<IHtmlAnchorElement>().Href.Replace("\t", "")
                    ))
                .ToDictionary(p => p.Serial, p => p.Source);//(setID + "-" + str.AsSpan().Slice(c => c.LastIndexOf('_') + 1, c => c.LastIndexOf(".")).ToString()).ToLower());

            /* Commented for future use
            Log.Information("Getting all PRs on card database without a YYT image link...");
            using (var db = _database())
            {
                var prCards = db.WeissSchwarzCards.AsAsyncEnumerable()
                    .Where(c => c.ReleaseID == releaseID
                                && c.Language == CardLanguage.English
                                && c.Rarity == "PR"
                                && !c.Images.Any(u => u.Authority == "jktcg.com")
                    );
                await foreach (var prCard in prCards)
                {
                    if (cardImages.TryGetValue(prCard.Serial, out var urlLink))
                    {
                        var imgUrl = new Uri(urlLink);
                        prCard.Images.Add(imgUrl);
                        db.Update(prCard);
                        Log.Information("Attached to {serial}: {imgUrl}", prCard.Serial, urlLink);
                    }
                }
                await db.SaveChangesAsync();
            }
            */

            await foreach (var card in originalCards)
            {
                var res = card.Clone();
                try
                {
                    var imgURL = cardImages[res.Serial.ToLower()];
                    res.Images.Add(new Uri(imgURL));
                    Log.Information("Attached image to {serial}: {imgURL}", res.Serial, imgURL);
                }
                catch (KeyNotFoundException)
                {
                    Log.Warning("Tried to post-process {serial} when the URL for {releaseID} was loaded. Skipping.", res.Serial, releaseID);
                }
                yield return res;
            }

            Log.Information("Finished.");
            yield break;
        }

        private object GetSerial(IHtmlImageElement ele)
        {
            Log.Debug("Parent Content: {content}", ele.Parent.TextContent);
            Log.Debug("Last Ancestor Content: {content}", ele.Ancestors().Last().Text());
            var innerHTML = ele.ParentElement.ParentElement.InnerHtml;
            return innerHTML.AsSpan().Slice(c => 1, c => c.Slice(1).IndexOf('\t') + 1).ToString().ToLower();
        }

        private async Task<(string setLinkWithUnderscores, string url)?> GetSetListURI(WeissSchwarzCard firstCard)
        {
            var menu = await "http://jktcg.com/MenuLeftEN.html"
                .WithHTMLHeaders()
                .GetHTMLAsync();
            return CardListURLFrom(menu, firstCard);
        }

        private (string setLinkWithUnderscores, string url)? CardListURLFrom(IDocument menu, WeissSchwarzCard firstCard)
        {
            var ogReleaseID = firstCard.ReleaseID;
            var releaseIDs = new List<string>();
            releaseIDs.Add(firstCard.ReleaseID);
            if (ogReleaseID.StartsWith("EN-")) releaseIDs.Add(ogReleaseID.Substring(3));
            var setLink = menu.Links.Cast<IHtmlAnchorElement>()
                                        .Where(ele => LinkMatcher.IsMatch(ele.Href))
                                        .Where(ele => releaseIDs.Any(s => ele.Href.Contains(s)))
                                        .FirstOrDefault();
            if (setLink == null)
            //{
            //    Log.Error("Cannot find a link that matches {SID} using this list of links: {@items}", ogReleaseID, menu.Links.Cast<IHtmlAnchorElement>().Select(ele => ele.Href).ToList());
                return null;
            //}
            
            var enPreString = "EN-";
            var setLinkWithUnderscores = setLink.Href.AsSpan()
                .Slice(x => x.IndexOf(enPreString))
                .ToString()
                .Replace("-", "_");

            return (setLinkWithUnderscores, $"http://jktcg.com/WS_EN/{setLinkWithUnderscores}/{setLinkWithUnderscores}.html");
        }
    }
}
