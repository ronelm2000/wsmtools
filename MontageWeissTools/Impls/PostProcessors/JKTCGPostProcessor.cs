using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public class JKTCGPostProcessor : ICardPostProcessor<WeissSchwarzCard>, ISkippable<IParseInfo>
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

    public async Task<bool> IsIncluded(IParseInfo info)
    {
        await Task.CompletedTask;

        if (info.ParserHints.Contains("skip:external", StringComparer.CurrentCultureIgnoreCase))
        {
            Log.Information("Skipping due to parser hint [skip:external].");
            return false;
        } else
        {
            return true;
        }
    }

    public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
    {
        await ValueTask.CompletedTask;
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

        var setList = await GetSetListURI(firstCard);
        if (!setList.HasValue)
        {
            Log.Information("Unable to find info from JKTCG; likely a new set, will skip.");
            return false;
        }
        else
        {
            return true;
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards, IProgress<PostProcessorProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var firstCard = await originalCards.FirstAsync();
        var lang = firstCard.Language;
        var stream = (lang == CardLanguage.Japanese) ? originalCards : Process(firstCard, new PostProcessorInfo {
            CancellationToken = cancellationToken,
            Progress = progress,
            OriginalCards = originalCards,
            ProgressReport = new PostProcessorProgressReport() { Percentage = 0, ReportMessage = new MultiLanguageString { EN = $"Starting JKTCG Post-Processor..." } }
        });

        await foreach (var card in stream)
        {
            yield return card;
        }
    }

    private async IAsyncEnumerable<WeissSchwarzCard> Process(WeissSchwarzCard firstCard, PostProcessorInfo info)
    {
        Log.Information("Starting...");
        (string setLinkWithUnderscores, string url)? pair = await GetSetListURI(firstCard);
        var originalCards = info.OriginalCards;
        var ct = info.CancellationToken;
        var progressReport = info.ProgressReport;
        var progress = info.Progress;
        var cardList = await pair?.url
            .WithHTMLHeaders()
            .GetHTMLAsync(ct);
        var releaseID = firstCard.ReleaseID;
        var cardImages = cardList.QuerySelectorAll<IHtmlImageElement>("a > img")
            .Select(ele => (
                Serial: GetSerial(ele),
                Source: ele.GetAncestor<IHtmlAnchorElement>().Href.Replace("\t", "")
                ))
            .ToDictionary(p => p.Serial, p => p.Source);//(setID + "-" + str.AsSpan().Slice(c => c.LastIndexOf('_') + 1, c => c.LastIndexOf(".")).ToString()).ToLower());

        await foreach (var card in originalCards)
        {
            ct.ThrowIfCancellationRequested();
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
            progressReport = progressReport.WithProcessedSerial(card, "JKTCG");
            progress.Report(progressReport);
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

    private async Task<(string setLinkWithUnderscores, string url)?> GetSetListURI(WeissSchwarzCard firstCard, CancellationToken token = default)
    {
        try
        {
            var menu = await "http://jktcg.com/MenuLeftEN.html"
                .WithHTMLHeaders()
                .GetHTMLAsync(token);
            return CardListURLFrom(menu, firstCard);
        }
        catch (Exception)
        {
            return null;
        }
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

    private record PostProcessorInfo
    {
        public IAsyncEnumerable<WeissSchwarzCard> OriginalCards { get; init; } = default;
        public IProgress<PostProcessorProgressReport> Progress { get; init; } = default;
        public PostProcessorProgressReport ProgressReport { get; init; } = default;
        public CancellationToken CancellationToken { get; init; } = default;

    }
}
