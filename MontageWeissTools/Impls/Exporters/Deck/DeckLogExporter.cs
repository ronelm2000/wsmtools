using Flurl.Http;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;
public class DeckLogExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>
{
    private readonly static ILogger Log = Serilog.Log.ForContext<DeckLogExporter>();

    private readonly Func<CardDatabaseContext> _db;
    private readonly Func<GlobalCookieJar> _cookieJar;
    private readonly Func<IFileOutCommandProcessor> _fileCommander;

    public string[] Alias => new[] { "decklog", "dl" };

    public DeckLogExporter(IContainer ioc)
    {
        _db = () => ioc.GetInstance<CardDatabaseContext>();
        _cookieJar = () => ioc.GetInstance<GlobalCookieJar>();
        _fileCommander = () => ioc.GetInstance<IFileOutCommandProcessor>();
    }

    public bool IsIncluded(IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard> inspector)
    {
        return inspector switch
        {
            SanityImageInspector _ => false,
            CachedImageInspector _ => false,
            SanityTranslationsInspector _ => false,
            _ => true
        };
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        var languages = deck.Ratios.Keys.Select(c => c.Language).Distinct();
        var reportStatus = DeckExportProgressReport.Starting(deck.Name, "DeckLog");
        if (languages.Count() > 1)
        {
            info.Progress.Report(reportStatus with { ReportMessage = new() { EN = "Error: Cannot support multiple language decks." } });
            return;
        }
        var cardTypes = deck.Ratios.Keys.Select(c => c.EnglishSetType).Distinct();
        if (cardTypes.Contains(EnglishSetType.Custom))
        {
            info.Progress.Report(reportStatus with { ReportMessage = new() { EN = "Error: Cannot Custom Weiss Schwarz cards." } });
            return;
        }

        var lang = languages.First();
        var deckLog = (lang == CardLanguage.Japanese) ? DeckLogSettings.Japanese : DeckLogSettings.English;
        var cookieSession = _cookieJar()[deckLog.Authority];
        var deckCreationRequest = await GenerateDeckCreationRequest(deckLog, deck);

        if (string.IsNullOrEmpty(deckCreationRequest.DeckParam2))
        {
            info.Progress.Report(reportStatus with { ReportMessage = new() { EN = $"Encountered Issue: Failed to match to a Neo-Standard specification." } });
            return;
        }

        Log.Information("Generating Deck Creation Tokens...");
        var createPost = await deckLog.CreateURL
            .WithCookies(cookieSession)
            .WithHeaders(new
            {
                Referer = deckLog.Referrer
            })
            .PostAsync(cancellationToken: cancellationToken);
        var deckCreationTokenResult = await createPost.GetJsonAsync<DeckCreationTokenResult>();

        Log.Information("Performing DeckLog's Deck Check...");
        info.Progress.Report(reportStatus = reportStatus with { Percentage = 33, ReportMessage = new() { EN = "Performing DeckLog's Deck Check..." } });

        var checkResultPost = await deckLog.DeckCheckURL
            .WithCookies(cookieSession)
            .WithHeaders(new
            {
                Referer = deckLog.Referrer
            })
            .PostJsonAsync(deckCreationRequest, cancellationToken: cancellationToken);

        var checkResult = await checkResultPost.GetJsonAsync<DeckLogCheckResult>();
        var deckErrors = checkResult.Errors
            .Concat(checkResult.Require)
            .Concat(checkResult.RecipeRequire)
            .Concat(checkResult.PopupRequire)
            .ToList();

        if (deckErrors.Count > 0)
        {
            var printedErrors = deckErrors.ConcatAsString("\n");
            Log.Error("Errors Encountered During Deck Check: \n{errors}", printedErrors);
            info.Progress.Report(reportStatus with { ReportMessage = new() { EN = $"Encountered Issue: {deckErrors[0]} {(deckErrors.Count > 1 ? " (and more...)" : "")}" } });
            return;
        }

        deckCreationRequest = deckCreationRequest with
        {
            DeckId = "",
            Token = deckCreationTokenResult.Token,
            TokenId = deckCreationTokenResult.TokenID
        };

        reportStatus = reportStatus with { Percentage = 66, ReportMessage = new() { EN = "Publishing to DeckLog..." } };
        info.Progress.Report(reportStatus);

        var publishResultPost = await deckLog.DeckPublishURL
            .WithCookies(cookieSession)
            .WithHeaders(new
            {
                Referer = deckLog.Referrer
            })
            .PostJsonAsync(deckCreationRequest, cancellationToken: cancellationToken);

        var publishResult = await publishResultPost.GetJsonAsync<DeckLogPublishResult>();

        if (publishResult.Status != "OK")
        {
            Log.Error("Publish Result is not OK; this shouldn't ever happen as it has been deck checked already!");
            Log.Error("Result: {@result}", publishResult);
            Log.Error("Please report this to dev at GitHub.");
            info.Progress.Report(reportStatus with { ReportMessage = new() { EN = "Result Failed! (This shouldn't happen.) Please report this to GitHub! " } });
            return;
        }

        var finalURL = deckLog.DeckFriendlyViewURL + publishResult.DeckID;
        Log.Information("Success!");
        Log.Information("URL: {url}", finalURL);

        reportStatus = reportStatus with
        {
            Percentage = 100,
            ReportMessage = new()
            {
                EN = $"Success! URL: {finalURL}"
            }
        };
        info.Progress.Report(reportStatus);

        await _fileCommander().OpenURL(finalURL);
    }

    private async Task<DeckLogDeckCheckQuery> GenerateDeckCreationRequest(DeckLogSettings deckLog, WeissSchwarzDeck deck)
    {
        var titleCodes = deck.Ratios.Keys
            .Select(c => c.TitleCode)
            .Distinct()
            .ToList();

        Log.Information("Accessing Suggest URL to check best matching Neo-Standard Specification...");
        var cardParams = await deckLog.SuggestURL
            .WithRESTHeaders()
            .WithReferrer(deckLog.Referrer)
            .WithCookies(_cookieJar()[deckLog.Referrer])
            .PostJsonAsync(new { Param = "" })
            .ReceiveJson<Dictionary<string, string>>();

        Log.Information("Match: {@asaa}", cardParams.Values);

        var matchingNsCode = cardParams.Keys
                .Select(s => (s, s.Split("##", StringSplitOptions.RemoveEmptyEntries)))
                .Where(p => p.Item2.Intersect(titleCodes).Count() == titleCodes.Count)
                .Select(p => p.s)
                .FirstOrDefault();

        Log.Information("Matched Neo-Standard Specification: {nsCode}", matchingNsCode ?? "None");
        
        if (matchingNsCode is null)
        {
            Log.Warning("No matching Neo-Standard Specification found. DeckLog may reject this deck during publication.");
        }

        var result = new DeckLogDeckCheckQuery(matchingNsCode ?? "", deck.Name);

        foreach (var pair in deck.Ratios)
        {
            result.No.Add(pair.Key.Serial);
            result.Num.Add(pair.Value);
        }

        return result;
    }
}

public record DeckLogDeckCheckQuery
{
    [JsonPropertyName("add_param1")]
    public string AddParam1 { get; } = "";

    [JsonPropertyName("add_param2")]
    public string AddParam2 { get; } = "";

    [JsonPropertyName("deck_param1")]
    public string DeckParam1 { get; } = "N"; // Neo-Standard
    [JsonPropertyName("deck_param2")]
    public string DeckParam2 { get; init; }
    [JsonPropertyName("has_session")]
    public bool HasSession { get; } = false;
    [JsonPropertyName("id")]
    public string ID { get; } = "";
    [JsonPropertyName("memo")]
    public string Memo { get; } = "";
    [JsonPropertyName("post_deckrecipe")]
    public int PostDeckRecipe { get; } = 1;
    [JsonPropertyName("no")]
    public List<string> No { get; } = new List<string>();
    [JsonPropertyName("num")]
    public List<int> Num { get; } = new List<int>();
    [JsonPropertyName("p_no")]
    public Object[] PNo { get; init; } = Array.Empty<object>();
    [JsonPropertyName("p_num")]
    public Object[] PNum { get; init; } = Array.Empty<object>();
    [JsonPropertyName("p_slot")]
    public Object[] PSlot { get; init; } = Array.Empty<object>();
    [JsonPropertyName("sub_no")]
    public Object[] SubNo { get; init; } = Array.Empty<object>();
    [JsonPropertyName("sub_num")]
    public Object[] SubNum { get; init; } = Array.Empty<object>();
    [JsonPropertyName("title")]
    public string Title { get; init; }

    // For Publishing
    [JsonPropertyName("deck_id")]
    public String? DeckId { get; init; } = null;
    [JsonPropertyName("token")]
    public String? Token { get; init; } = null;

    [JsonPropertyName("token_id")]
    public String? TokenId { get; init; } = null;

    internal DeckLogDeckCheckQuery(string nsSearchTerm, string title)
    {
        this.DeckParam2 = nsSearchTerm;
        this.Title = title;
    }
    
}

public record DeckLogCheckResult
{
    [JsonPropertyName("require")]
    public List<string> Require { get; init; } = new List<string>();

    [JsonPropertyName("recipe_require")]
    public List<string> RecipeRequire { get; init; } = new List<string>();

    [JsonPropertyName("popup_require")]
    public List<string> PopupRequire { get; init; } = new List<string>();

    [JsonPropertyName("error")]
    public List<string> Errors { get; init; } = new List<string>();

    [JsonPropertyName("warning")]
    public List<string> Warnings { get; init; }  = new List<string>();

    [JsonPropertyName("curr_deck_count")]
    public int CurrDeckCount { get; init; } = 0;

    [JsonPropertyName("version")]
    public String Version { get; init; } = "";
}

public record DeckLogPublishResult
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = "";

    [JsonPropertyName("id")]
    public int ID { get; init; }

    [JsonPropertyName("deck_id")]
    public string DeckID { get; init; } = "";
}

public record DeckCreationTokenResult
{
    [JsonPropertyName("token_id")]
    public string TokenID { get; init; } = "";
    [JsonPropertyName("token")]
    public string Token { get; init; } = "";
}