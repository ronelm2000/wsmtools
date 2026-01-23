using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Impls.Utilities;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
internal interface IDeckLogClient
{
    Task<bool> IsCompatible(WeissSchwarzCard card);
    Task<bool> IsCompatible(CardLanguage language, CardSide side);
    IAsyncEnumerable<DLCardEntry> FindCardEntries(
        DeckLogContext context,
        string nsCodes, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default
        );
}
#pragma warning restore CS8424

internal record DeckLogContext
{
    public CardLanguage Language { get; init; } = CardLanguage.Japanese;
    public required string Authority { get; init; }
    public required ICachedMapService<(CardLanguage, string), Dictionary<string, DLCardEntry>> CacheService { get; init; }
    public required GlobalCookieJar CookieJar { get; init; }

    public void Deconstruct(
        out CardLanguage language,
        out string authority,
        out ICachedMapService<(CardLanguage, string), Dictionary<string, DLCardEntry>> cacheService,
        out GlobalCookieJar cookieJar
        )
    {
        language = Language;
        authority = Authority;
        cacheService = CacheService;
        cookieJar = CookieJar;
    }
}

internal class DLCardEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("num")]
    public int Number { get; set; }

    [JsonPropertyName("card_kind")]
    public int CardType { get; set; }

    [JsonPropertyName("card_number")]
    required public string Serial { get; set; }

    [JsonPropertyName("name")]
    required public string Name { get; set; }

    [JsonPropertyName("rare")]
    required public string Rarity { get; set; }

    [JsonPropertyName("img")]
    required public string ImagePath { get; set; }
}

internal class DLQueryParameters
{
    [JsonPropertyName("title_number_select_for_search")]
    public Dictionary<string, TitleSelection> TitleSelectionsForSearch { get; set; } = new();

    public IEnumerable<string[]> GetTitleSelectionKeys()
    {
        return TitleSelectionsForSearch.Keys.Select(s => s.Split("##", StringSplitOptions.RemoveEmptyEntries));
    }
}

internal class TitleSelection
{
    public int Side { get; set; } // -1 = Weiss ; -2 = Schwarz ; -3 = Both
    public string Label { get; set; } = string.Empty;
    public int ID { get; set; }
}

internal class DLCardQuery
{
    [JsonPropertyName("title_number")]
    public string Titles { get; set; } = "";
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = "";
    [JsonPropertyName("keyword_type")]
    public string[] KeywordQueryType { get; set; } = new string[] { "name", "text", "no", "feature" };
    [JsonPropertyName("side")]
    public string Side { get; set; } = "";
    [JsonPropertyName("card_kind")]
    public CardType TypeQuery { get; set; } = CardType.All;
    [JsonPropertyName("color")]
    public CardColor ColorQuery { get; set; } = CardColor.All;
    [JsonPropertyName("parallel")]
    public string Parallel { get; set; } = "";
    [JsonPropertyName("option_clock")]
    public bool CounterCardsOnly { get; set; } = false;
    [JsonPropertyName("option_counter")]
    public bool ClockCardsOnly { get; set; } = false;
    [JsonPropertyName("deck_param1")]
    public DeckConstructionType DeckConstruction { get; set; }

    [JsonPropertyName("deck_param2")]
    public string DeckConstructionParameter { get; set; } = "";

    [JsonPropertyName("cost_e")]
    public string CostEnd = "";

    [JsonPropertyName("cost_s")]
    public string CostStart = "";

    [JsonPropertyName("level_e")]
    public string LevelStart = "";

    [JsonPropertyName("level_s")]
    public string LevelEnd = "";

    [JsonPropertyName("power_e")]
    public string PowerEnd = "";

    [JsonPropertyName("power_s")]
    public string PowerStart = "";

    [JsonPropertyName("soul_e")]
    public string SoulEnd = "";

    [JsonPropertyName("soul_s")]
    public string SoulStart = "";

    [JsonPropertyName("trigger")]
    public string Trigger = ""; //TODO: Replace this with string--based enum.

    //TODO: There's actually alot of missing variables that can be placed here, but these are ignored for now.
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[DataContract]
internal enum CardType
{
    [JsonStringEnumMemberName("0")]
    All,
    [JsonStringEnumMemberName("2")]
    Character,
    [JsonStringEnumMemberName("3")]
    Event,
    [JsonStringEnumMemberName("4")]
    Climax
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[DataContract]
internal enum CardColor
{
    [JsonStringEnumMemberName("0")]
    All,
    [JsonStringEnumMemberName("yellow")]
    Yellow,
    [JsonStringEnumMemberName("green")]
    Green,
    [JsonStringEnumMemberName("red")]
    Red,
    [JsonStringEnumMemberName("blue")]
    Blue
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[DataContract]
internal enum DeckConstructionType
{
    [JsonStringEnumMemberName("S")]
    Standard,
    [JsonStringEnumMemberName("N")]
    NeoStandard,
    [JsonStringEnumMemberName("T")]
    TitleOnly,
    [JsonStringEnumMemberName("O")]
    Others
}