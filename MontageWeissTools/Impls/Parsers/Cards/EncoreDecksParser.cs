using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards;

/// <summary>
/// Parses card set results from the EncoreDecks API. Thanks to the dev who made this API for consumption.
/// </summary>
public class EncoreDecksParser : ICardSetParser<WeissSchwarzCard>
{
    private readonly Regex encoreDecksAPIMatcher = new Regex(@"http(?:s)?:\/\/www\.encoredecks\.com\/api\/series\/(.+)\/cards");
    private readonly Regex encoreDecksSiteSetMatcher = new Regex(@"http(?:s)?:\/\/www.encoredecks\.com\/?.+&set=([a-f0-9]+).*");
    private readonly ILogger Log = Serilog.Log.ForContext<EncoreDecksParser>();

    public async Task<bool> IsCompatible(IParseInfo info)
    {
        var urlOrFile = info.URI;
        if (encoreDecksAPIMatcher.IsMatch(urlOrFile))
        {
            Log.Information("Compatibility Passed for: {urlOrFile}", urlOrFile);
            return await ValueTask.FromResult(true);
        }
        else if (encoreDecksSiteSetMatcher.IsMatch(urlOrFile))
        {
            Log.Information("Compatibility Passed for: {urlOrFile}", urlOrFile);
            return await ValueTask.FromResult(true);
        }
        else
        {
            Log.Debug("Compatibility Failed for: {urlOrFile}", urlOrFile);
            return await ValueTask.FromResult(false);
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile, IProgress<SetParserProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (encoreDecksSiteSetMatcher.IsMatch(urlOrLocalFile))
            urlOrLocalFile = TransformIntoAPIFormat(urlOrLocalFile);

        var progressReport = new SetParserProgressReport
        {
            ReportMessage = new MultiLanguageString
            {
                EN = "Obtaining list of cards..."
            },
            Percentage = 1
        };
        progress.Report(progressReport);
        var setCards = await urlOrLocalFile.WithRESTHeaders().GetJsonAsync<List<EncoreDeckCard>>(cancellationToken: cancellationToken);

        progressReport = progressReport with {
            ReportMessage = new MultiLanguageString { EN = $"Obtained [{setCards.Count}] cards." }, 
            Percentage = 10
        };
        progress.Report(progressReport);

        var setCardResults = setCards.ToAsyncEnumerable().SelectParallelAsync(this.Decode, cancellationToken: cancellationToken);
        await foreach (var result in setCardResults)
        {
            progressReport = progressReport with
            {
                ReportMessage = new MultiLanguageString { EN = $"Parsed [{result.Serial}]." },
                Percentage = 10 + (int)((progressReport.CardsParsed + 1f) * 90 / setCards.Count),
                CardsParsed = progressReport.CardsParsed + 1
            };
            progress.Report(progressReport);
            yield return result;
        }
    }

    private async Task<WeissSchwarzCard> Decode(EncoreDeckCard setCard)
    {
        WeissSchwarzCard result = new WeissSchwarzCard();
        result.Name = new MultiLanguageString();
        var enOptional = setCard.Locale.EN;
        var jpOptional = setCard.Locale.NP;
        if (enOptional?.Source?.ToLower() != "akiba")
            result.Name.EN = enOptional!.Name;
        result.Name.JP = jpOptional?.Name;
        (List<string>?, List<string>?) attributes = (enOptional?.Attributes, jpOptional?.Attributes);
        result.Traits = TranslateTraits(attributes).ToList();
        result.Effect = enOptional?.Ability?.ToArray() ?? Array.Empty<string>();
        result.Rarity = setCard.Rarity!;
        result.Side = TranslateSide(setCard.Side);
        result.Level = setCard.Level;
        result.Cost = setCard.Cost;
        result.Power = setCard.Power;
        result.Soul = setCard.Soul;
        result.Triggers = TranslateTriggers(setCard.Trigger ?? Enumerable.Empty<String>());

        //result.Serial = setCard.cardcode;
        if (!String.IsNullOrEmpty(setCard.ImagePath))
            result.Images.Add(new Uri($"https://www.encoredecks.com/images/{setCard.ImagePath}"));

        // TODO: Delete all methods related with generating serial.
        // TODO: Switch once LLDX checkbox is checked properly. See: https://trello.com/c/WCT94Sk0/2-card-code-needs-to-be-stored-seperatly-from-side-release
        result.Serial = WeissSchwarzCard.GetSerial(setCard.Set!, setCard.Side!, setCard.Lang!, setCard.Release!, setCard.SID!);

        result.Type = TranslateType(setCard.CardType!);
        result.Color = TranslateColor(setCard.Colour!);
        result.Remarks = $"Parsed: {this.GetType().Name}";

        result = FixSiteErrata(result);
        return await ValueTask.FromResult(result);
    }

    private string TransformIntoAPIFormat(string urlOrFile)
    {
        Log.Information("Converting URL into API link...");
        return TransformIntoAPIFormatFromSetGUID(encoreDecksSiteSetMatcher.Match(urlOrFile).Groups[1].Value);
    }

    private string TransformIntoAPIFormatFromSetGUID(string setGUID)
        => $"https://www.encoredecks.com/api/series/{setGUID}/cards";

    private CardColor TranslateColor(string color)
    {
        return color switch
        {
            "YELLOW" => CardColor.Yellow,
            "GREEN" => CardColor.Green,
            "BLUE" => CardColor.Blue,
            "RED" => CardColor.Red,
            "PURPLE" => CardColor.Purple,
            "色：0" => CardColor.Yellow,
            "色：1" => CardColor.Green,
            "色：2" => CardColor.Red,
            "色：3" => CardColor.Blue,
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

    private CardSide TranslateSide(string? side)
    {
        return side switch
        {
            "W" => CardSide.Weiss,
            "S" => CardSide.Schwarz,
            _ => CardSide.Both //TODO: This should be changed to the proper value for both, and the rest of the values will display an error.
        };
    }

    private IEnumerable<WeissSchwarzTrait> TranslateTraits((List<string>? EN, List<string>? JP) attributes)
    {
        var nullCheckedAttributes = (
            EN: attributes.EN ?? Enumerable.Empty<string>().ToList(),
            JP: attributes.JP ?? Enumerable.Empty<string>().ToList()
            );
        var maxlength = Math.Max(nullCheckedAttributes.EN.Count, nullCheckedAttributes.JP.Count);
        var enSpan = nullCheckedAttributes.EN.ToArray();
        var jpSpan = nullCheckedAttributes.JP.ToArray();
        var maxLength = Math.Max(enSpan.Length, jpSpan.Length);
        Array.Resize(ref enSpan, maxLength);
        Array.Resize(ref jpSpan, maxLength);
        return enSpan.Zip(jpSpan, Construct)
            .Where(mls => mls is not null)
            .Select(mls => mls!);
    }

    private static readonly string[] NULL_TRAITS = new[]
    {
        "-",
        "0",
        "－",
        "ー",
        "―"
    };

    private WeissSchwarzTrait? Construct(string? traitEN, string? traitJP)
    {
        WeissSchwarzTrait str = new WeissSchwarzTrait();
        str.EN = traitEN?.ToString();
        str.JP = traitJP?.ToString();
        str.EN = (String.IsNullOrWhiteSpace(str.EN) || NULL_TRAITS.Contains(str.EN)) ? null : str.EN;
        str.JP = (String.IsNullOrWhiteSpace(str.JP) || NULL_TRAITS.Contains(str.JP)) ? null : str.JP;
        if (str.EN is null && str.JP is null)
            return null;
        else
            return str;
    }

    private Trigger[] TranslateTriggers(IEnumerable<string> triggers) => triggers.Select(o => o ?? string.Empty).Select(TranslateTrigger).ToArray();

    private Trigger TranslateTrigger(string trigger)
    {
        return trigger.ToLower() switch
        {
            // Yellow
            "soul" => Trigger.Soul,
            "shot" => Trigger.Shot,
            "return" => Trigger.Bounce,
            "wind" => Trigger.Bounce,
            "choice" => Trigger.Choice,
            // Green
            "treasure" => Trigger.GoldBar,
            "bag" => Trigger.Bag,
            "pool" => Trigger.Bag,
            // Red
            "comeback" => Trigger.Door,
            "salvage" => Trigger.Door,
            "standby" => Trigger.Standby,
            // Blue
            "draw" => Trigger.Book,
            "gate" => Trigger.Gate,
            var str => throw new Exception($"Cannot parse {typeof(Trigger).Name} from {str}")
        };
    }

    private WeissSchwarzCard FixSiteErrata(WeissSchwarzCard originalCard)
    {
        return originalCard switch
        {
            { Serial: "P4/EN-S01-T06" } => originalCard.WithTrait(new [] {
                new WeissSchwarzTrait() { EN = "Junes", JP = "Junes" },
                new WeissSchwarzTrait() { EN = "Magic", JP = "Magic" }
            }),
            { Serial: "P4/EN-S01-T08" } => originalCard.WithTrait(new[]
            {
                new WeissSchwarzTrait() { EN = "Junes", JP = "Junes" },
                new WeissSchwarzTrait() { EN = "Magic", JP = "Magic" }
            } ),
            _ => originalCard
        };
    }
}

internal record EncoreDeckCard
{
    [JsonPropertyName("cardcode")]
    public required string Serial { get; init; }

    [JsonPropertyName("locale")]
    public required LocaleRecords Locale { get; init; }
    
    public string? Rarity { get; init; }
    public string? Side { get; init; }
    public int? Level { get; init; }
    public int? Cost { get; init; }
    public int? Power { get; init; }
    public int? Soul { get; init; }
    public List<string>? Trigger { get; init; }
    public string? ImagePath { get; init; }
    public string? CardType { get; init; }
    public string? Colour { get; init; }
    public string? Set { get; init; }
    public string? Lang { get; init; }
    public string? Release { get; init; }
    public string? SID { get; init; }
}

internal record LocaleRecords
{
    public LocaleRecord? EN { get; init; }
    public LocaleRecord? NP { get; init; }
}

internal record LocaleRecord
{
    public string? Source { get; init; }
    public string? Name { get; init; }
    public List<string>? Attributes { get; init; }
    public List<string>? Ability { get; init; }
}