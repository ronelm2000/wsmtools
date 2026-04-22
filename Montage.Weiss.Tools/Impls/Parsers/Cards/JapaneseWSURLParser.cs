using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Utilities;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards;

/// <summary>
/// Parses Japanese Weiss Schwarz card sets from ws-tcg.com search results.
/// </summary>
public class JapaneseWSURLParser : ICardSetParser<WeissSchwarzCard>, IFilter<ICardPostProcessor<WeissSchwarzCard>>
{
    private const string SupportedAuthority = "ws-tcg.com";
    private const string SupportedPath = "/cardlist/search/";
    private const string ApiUrlFormat = "https://ws-tcg.com/manage/CardListUser/searchJson?expansion={0}&page={1}";
    private const string CardImagePrefix = "https://ws-tcg.com/wordpress/wp-content/images/cardlist/";

    private static readonly ILogger Log = Serilog.Log.ForContext<JapaneseWSURLParser>();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, CardType> CardTypeMap = new(StringComparer.Ordinal)
    {
        ["2"] = CardType.Character,
        ["3"] = CardType.Event,
        ["4"] = CardType.Climax
    };

    private static readonly Dictionary<string, CardColor> CardColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["[[yellow.gif]]"] = CardColor.Yellow,
        ["[[green.gif]]"] = CardColor.Green,
        ["[[red.gif]]"] = CardColor.Red,
        ["[[blue.gif]]"] = CardColor.Blue,
        ["[[purple.gif]]"] = CardColor.Purple
    };

    private static readonly Dictionary<string, CardSide> CardSideMap = new(StringComparer.Ordinal)
    {
        ["-1"] = CardSide.Weiss,
        ["-2"] = CardSide.Schwarz,
        ["-3"] = CardSide.Both
    };

    private static readonly Dictionary<string, Trigger> TriggerMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["[[soul.gif]]"] = Trigger.Soul,
        ["[[bounce.gif]]"] = Trigger.Bounce,
        ["[[shot.gif]]"] = Trigger.Shot,
        ["[[choice.gif]]"] = Trigger.Choice,
        ["[[treasure.gif]]"] = Trigger.GoldBar,
        ["[[stock.gif]]"] = Trigger.Bag,
        ["[[standby.gif]]"] = Trigger.Standby,
        ["[[comeback.gif]]"] = Trigger.Door,
        ["[[salvage.gif]]"] = Trigger.Door,
        ["[[gate.gif]]"] = Trigger.Gate,
        ["[[draw.gif]]"] = Trigger.Book,
        ["[[discover.gif]]"] = Trigger.Discover,
        ["[[chance.gif]]"] = Trigger.Chance,
        ["[[focus.gif]]"] = Trigger.Focus
    };

    private static readonly Regex TriggerRegex = new(@"\[\[[^]]+\.gif\]\]", RegexOptions.Compiled);
    private static readonly string[] LineBreakTokens = ["<br>", "<br/>", "<br />"];

    public async Task<bool> IsCompatible(IParseInfo parseInfo)
    {
        await ValueTask.CompletedTask;

        if (!TryGetExpansionId(parseInfo.URI, out _))
        {
            Log.Debug("Compatibility failed for {uri}", parseInfo.URI);
            return false;
        }

        Log.Information("Compatibility passed for {uri}", parseInfo.URI);
        return true;
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile, IProgress<SetParserProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!TryGetExpansionId(urlOrLocalFile, out var expansionId))
            throw new SetParsingException(new UnsupportedPreconditionCode("JP WS expansion search URL"));

        var progressReport = new SetParserProgressReport
        {
            ReportMessage = new MultiLanguageString { EN = $"Obtaining JP WS set [{expansionId}]..." },
            Percentage = 1
        };
        progress.Report(progressReport);

        var firstPage = await GetPage(expansionId, 1, cancellationToken);
        ValidatePage(firstPage, expansionId, 1);

        Log.Information("Loaded JP WS expansion {expansionId}. Total {total}. Pages {pageCount}.", expansionId, firstPage.Total, firstPage.PageCount);

        progressReport = progressReport with
        {
            ReportMessage = new MultiLanguageString { EN = $"Obtained [{firstPage.Total}] cards metadata from expansion [{expansionId}]." },
            Percentage = 10
        };
        progress.Report(progressReport);

        var totalCards = firstPage.Total;
        var parsedCards = 0;

        foreach (var item in firstPage.Items)
        {
            var card = DecodeCard(item);
            parsedCards++;
            progress.Report(progressReport = progressReport with
            {
                CardsParsed = parsedCards,
                Percentage = 10 + (int)(parsedCards * 90f / totalCards),
                ReportMessage = new MultiLanguageString { EN = $"Parsed [{card.Serial}]." }
            });
            yield return card;
        }

        for (var page = 2; page <= firstPage.PageCount; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPage = await GetPage(expansionId, page, cancellationToken);
            ValidatePage(currentPage, expansionId, page);

            foreach (var item in currentPage.Items)
            {
                var card = DecodeCard(item);
                parsedCards++;
                progress.Report(progressReport = progressReport with
                {
                    CardsParsed = parsedCards,
                    Percentage = 10 + (int)(parsedCards * 90f / totalCards),
                    ReportMessage = new MultiLanguageString { EN = $"Parsed [{card.Serial}]." }
                });
                yield return card;
            }
        }

        progress.Report(progressReport with
        {
            Percentage = 100,
            ReportMessage = new MultiLanguageString { EN = $"Parsed all cards from expansion [{expansionId}]." }
        });
    }

    private static bool TryGetExpansionId(string uriString, out string expansionId)
    {
        expansionId = string.Empty;

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Authority, SupportedAuthority, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(uri.AbsolutePath, SupportedPath, StringComparison.Ordinal))
            return false;

        expansionId = HttpUtility.ParseQueryString(uri.Query)["expansion"] ?? string.Empty;
        return !string.IsNullOrWhiteSpace(expansionId);
    }

    private static async Task<WsSearchResponse> GetPage(string expansionId, int page, CancellationToken cancellationToken)
    {
        var url = string.Format(ApiUrlFormat, expansionId, page);
        Log.Debug("Loading JP WS page {page}: {url}", page, url);

        var response = await url
            .WithRESTHeaders()
            .GetStringAsync(cancellationToken: cancellationToken);

        return JsonSerializer.Deserialize<WsSearchResponse>(response, JsonOptions)
            ?? throw new SetParsingException("JP WS API returned empty payload.");
    }

    private static void ValidatePage(WsSearchResponse? page, string expansionId, int pageNumber)
    {
        if (page is null)
            throw new SetParsingException($"JP WS API returned null page for expansion [{expansionId}] page [{pageNumber}].");

        if (page.Items is null)
            throw new SetParsingException($"JP WS API returned null items for expansion [{expansionId}] page [{pageNumber}].");

        if (page.Total < 0 || page.PageCount < 1 || page.Page < 1)
            throw new SetParsingException($"JP WS API returned invalid pagination for expansion [{expansionId}] page [{pageNumber}].");
    }

    private static WeissSchwarzCard DecodeCard(WsSearchCardItem item)
    {
        var serial = Require(item.CardNumber, nameof(item.CardNumber));
        var typeValue = Require(item.CardKind, nameof(item.CardKind));
        var colorValue = Require(item.Color, nameof(item.Color));
        var sideValue = Require(item.Side, nameof(item.Side));

        if (!CardTypeMap.TryGetValue(typeValue, out var cardType))
            throw new SetParsingException(new CannotBeParsedCode($"{nameof(CardType)} [{typeValue}]"));

        if (!CardColorMap.TryGetValue(colorValue, out var cardColor))
            throw new SetParsingException(new CannotBeParsedCode($"{nameof(CardColor)} [{colorValue}]"));

        if (!CardSideMap.TryGetValue(sideValue, out var cardSide))
            throw new SetParsingException(new CannotBeParsedCode($"{nameof(CardSide)} [{sideValue}]"));

        var card = new WeissSchwarzCard
        {
            Serial = serial,
            Name = new MultiLanguageString
            {
                JP = Require(item.CardName, nameof(item.CardName)),
                EN = string.Empty
            },
            Type = cardType,
            Color = cardColor,
            Side = cardSide,
            Rarity = CleanString(item.Rare),
            Level = ParseNullableInt(item.Level),
            Cost = ParseNullableInt(item.Cost),
            Power = ParseNullableInt(item.Power),
            Soul = ParseSymbols(item.Soul, Trigger.Soul).Length,
            Triggers = ParseTriggers(item.CardTrigger),
            Effect = ParseEffect(item.Text),
            Flavor = NormalizeHtmlText(item.Flavor),
            Remarks = $"Parsed: {nameof(JapaneseWSURLParser)}"
        };

        card.Traits = ParseTraits(item.Feature1, item.Feature2, item.Feature3).ToList();

        if (!string.IsNullOrWhiteSpace(item.Picture) && item.Picture != "-")
            card.Images.Add(new Uri(new Uri(CardImagePrefix), item.Picture));

        return card;
    }

    private static IEnumerable<WeissSchwarzTrait> ParseTraits(params string?[] features)
    {
        foreach (var feature in features.Select(CleanString).Where(f => !string.IsNullOrWhiteSpace(f) && f != "-"))
        {
            yield return new WeissSchwarzTrait
            {
                JP = feature,
                EN = string.Empty
            };
        }
    }

    private static Trigger[] ParseTriggers(string? value)
        => ParseSymbols(value)
            .Select(symbol =>
            {
                if (!TriggerMap.TryGetValue(symbol, out var trigger))
                    throw new SetParsingException(new CannotBeParsedCode($"{nameof(Trigger)} [{symbol}]"));
                return trigger;
            })
            .ToArray();

    private static string[] ParseSymbols(string? value, Trigger? filter = null)
    {
        var matches = TriggerRegex.Matches(value ?? string.Empty)
            .Select(match => match.Value);

        if (filter is null)
            return matches.ToArray();

        return matches
            .Where(symbol => TriggerMap.TryGetValue(symbol, out var mapped) && mapped == filter)
            .ToArray();
    }

    private static string[] ParseEffect(string? text)
        => SplitHtmlLines(text)
            .Select(NormalizeHtmlText)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

    private static IEnumerable<string> SplitHtmlLines(string? text)
    {
        var normalized = text ?? string.Empty;
        foreach (var lineBreakToken in LineBreakTokens)
            normalized = normalized.Replace(lineBreakToken, "\n", StringComparison.OrdinalIgnoreCase);

        return normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string NormalizeHtmlText(string? text)
    {
        var normalized = text ?? string.Empty;
        foreach (var lineBreakToken in LineBreakTokens)
            normalized = normalized.Replace(lineBreakToken, "\n", StringComparison.OrdinalIgnoreCase);

        return WebUtility.HtmlDecode(normalized).Trim();
    }

    private static int? ParseNullableInt(string? value)
    {
        var cleaned = CleanString(value);
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned == "-")
            return null;

        return int.TryParse(cleaned, out var result)
            ? result
            : throw new SetParsingException(new CannotBeParsedCode($"Int32 [{cleaned}]"));
    }

    private static string CleanString(string? value) => (value ?? string.Empty).Trim();

    private static string Require(string? value, string propertyName)
    {
        var cleaned = CleanString(value);
        return !string.IsNullOrWhiteSpace(cleaned)
            ? cleaned
            : throw new SetParsingException($"JP WS API field [{propertyName}] is required.");
    }

    public bool IsIncluded(ICardPostProcessor<WeissSchwarzCard> item)
    {
        return item is not DeckLogPostProcessor;
    }
}

internal sealed class WsSearchResponse
{
    [JsonPropertyName("items")]
    public List<WsSearchCardItem> Items { get; init; } = [];
    [JsonPropertyName("total")]
    public int Total { get; init; }
    [JsonPropertyName("page")]
    public int Page { get; init; }
    [JsonPropertyName("limit")]
    public int Limit { get; init; }
    [JsonPropertyName("page_count")]
    public int PageCount { get; init; }
}

internal sealed class WsSearchCardItem
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("card_number")]
    public string CardNumber { get; init; } = string.Empty;
    [JsonPropertyName("variation")]
    public int Variation { get; init; }
    [JsonPropertyName("title_number")]
    public string TitleNumber { get; init; } = string.Empty;
    [JsonPropertyName("card_name")]
    public string CardName { get; init; } = string.Empty;
    [JsonPropertyName("card_kind")]
    public string CardKind { get; init; } = string.Empty;
    [JsonPropertyName("color")]
    public string Color { get; init; } = string.Empty;
    [JsonPropertyName("level")]
    public string Level { get; init; } = string.Empty;
    [JsonPropertyName("cost")]
    public string Cost { get; init; } = string.Empty;
    [JsonPropertyName("power")]
    public string Power { get; init; } = string.Empty;
    [JsonPropertyName("soul")]
    public string Soul { get; init; } = string.Empty;
    [JsonPropertyName("card_trigger")]
    public string CardTrigger { get; init; } = string.Empty;
    [JsonPropertyName("parallel_param")]
    public string ParallelParam { get; init; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
    [JsonPropertyName("flavor")]
    public string Flavor { get; init; } = string.Empty;
    [JsonPropertyName("picture")]
    public string Picture { get; init; } = string.Empty;
    [JsonPropertyName("expansion")]
    public int Expansion { get; init; }
    [JsonPropertyName("rare")]
    public string Rare { get; init; } = string.Empty;
    [JsonPropertyName("feature1")]
    public string Feature1 { get; init; } = string.Empty;
    [JsonPropertyName("feature2")]
    public string Feature2 { get; init; } = string.Empty;
    [JsonPropertyName("feature3")]
    public string Feature3 { get; init; } = string.Empty;
    [JsonPropertyName("side")]
    public string Side { get; init; } = string.Empty;
}
