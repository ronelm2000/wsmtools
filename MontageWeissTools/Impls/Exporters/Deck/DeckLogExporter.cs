using Flurl.Http;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;
public class DeckLogExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>
{
    private readonly Func<CardDatabaseContext> _db;
    private readonly Func<GlobalCookieJar> _cookieJar;

    public string[] Alias => new[] { "decklog", "dl" };

    public DeckLogExporter(IContainer ioc)
    {
        _db = () => ioc.GetInstance<CardDatabaseContext>();
        _cookieJar = () => ioc.GetInstance<GlobalCookieJar>();
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
        if (languages.Count() > 1)
        {
            throw new NotImplementedException("Cannot support multiple language decks.");
        }
        var lang = languages.First();
        var deckLog = (lang == CardLanguage.Japanese) ? DeckLogSettings.Japanese : DeckLogSettings.English;
        var deckCreationRequest = GenerateDeckCreationRequest(deck);

        Log.Information("Checking for any inconsistencies via DeckLog...");
        var cookieSession = _cookieJar()[deckLog.Authority];
        var checkResultPost = await deckLog.DeckCheckURL
            .WithCookies(cookieSession)
            .WithHeaders(new
            {
                Referer = deckLog.Referrer
            })
            .PostJsonAsync(deckCreationRequest, cancellationToken: cancellationToken);

        var checkResult = await checkResultPost.GetJsonAsync<DeckLogCheckResult>();

        if (checkResult.Errors.Count() > 0)
        {
            var printedErrors = checkResult.Errors
                .Select(o => o?.ToString() ?? "")
                .Where(s => !String.IsNullOrEmpty(s))
                .ConcatAsString("\n");
            Log.Error("Errors Encountered During Deck Check: \n{errors}", printedErrors);
            return;
        }

        deckCreationRequest = deckCreationRequest with
        {
            Token = "",
            TokenId = ""
        };

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
            return;
        }

        Log.Information("Success!");
        Log.Information("URL: {url}", (deckLog.DeckFriendlyViewURL + publishResult.DeckID));
    }

    private DeckLogDeckCheckQuery GenerateDeckCreationRequest(WeissSchwarzDeck deck)
    {
        var nsCodes = deck.Ratios.Keys
            .Select(c => c.TitleCode)
            .Distinct();
        var result = new DeckLogDeckCheckQuery($"##{nsCodes.ConcatAsString("##")}##", deck.Name);

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
    [JsonProperty("add_param1")]
    public string AddParam1 { get; } = "";

    [JsonProperty("add_param2")]
    public string AddParam2 { get; } = "";

    [JsonProperty("deck_param1")]
    public string DeckParam1 { get; } = "N"; // Neo-Standard
    [JsonProperty("deck_param2")]
    public string DeckParam2 { get; init; }
    [JsonProperty("has_session")]
    public bool HasSession { get; } = false;
    [JsonProperty("id")]
    public string ID { get; } = "";
    [JsonProperty("memo")]
    public string memo { get; } = "";

    [JsonProperty("no")]
    public List<string> No { get; } = new List<string>();
    [JsonProperty("num")]
    public List<int> Num { get; } = new List<int>();

    [JsonProperty("p_no")]
    public Object[] PNo = Array.Empty<object>();
    [JsonProperty("p_num")]
    public Object[] PNum = Array.Empty<object>();
    [JsonProperty("sub_no")]
    public Object[] SubNo = Array.Empty<object>();
    [JsonProperty("sub_num")]
    public Object[] SubNum = Array.Empty<object>();


    [JsonProperty("title")]
    public string Title;

    // For Publishing
    [JsonProperty("token")]
    public String? Token;

    [JsonProperty("token_id")]
    public String? TokenId;

    internal DeckLogDeckCheckQuery(string nsSearchTerm, string title)
    {
        this.DeckParam2 = nsSearchTerm;
        this.Title = title;
    }
    
}

public record DeckLogCheckResult
{
    [JsonProperty("require")]
    public List<string> Require { get; init; } = new List<string>();

    [JsonProperty("error")]
    public List<string> Errors { get; init; } = new List<string>();

    [JsonProperty("warning")]
    public List<string> Warnings { get; init; }  = new List<string>();

    [JsonProperty("curr_deck_count")]
    public int CurrDeckCount { get; init; } = 0;

    [JsonProperty("version")]
    public String Version { get; init; } = "";
}

public record DeckLogPublishResult
{
    [JsonProperty("status")]
    public string Status { get; init; } = "";

    [JsonProperty("id")]
    public int ID { get; init; }

    [JsonProperty("deck_id")]
    public string DeckID { get; init; } = "";
}