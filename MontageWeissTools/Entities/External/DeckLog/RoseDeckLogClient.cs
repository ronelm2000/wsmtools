using Flurl.Http;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

internal class RoseDeckLogClient : IDeckLogClient, ICardQueryable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<OriginalDeckLogClient>();

    public async Task<bool> IsCompatible(WeissSchwarzCard card, CancellationToken cancellationToken = default)
    {
        return await IsCompatible(card.Language, card.Side, cancellationToken);
    }

    public async Task<bool> IsCompatible(CardLanguage language, CardSide side, CancellationToken cancellationToken = default)
    {
        if (side == CardSide.Rose && language == CardLanguage.Japanese)
        {
            return await ValueTask.FromResult(true);
        } else
        {
            return false;
        }
    }

    public async IAsyncEnumerable<DLCardEntry> FindCardEntries(DeckLogContext context, string nsCode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (_, _, _, cookieJarSrvc) = context;
        var setting = DeckLogSettings.Rose;
        var query = new DLCardQuery
        {
            DeckConstruction = DeckConstructionType.Others,
            Keyword = $"{nsCode}/",
            KeywordQueryType = ["no"]
        };
        var page = 1;
        var cookieJar = await cookieJarSrvc.FindOrCreate(setting.Authority, cancellationToken);

        List<DLCardEntryV2> cardEntryList;
        do
        {
            Log.Information("Extracting Page {pagenumber}...", page);
            cardEntryList = await setting.SearchURL
                .WithRESTHeaders()
                .WithReferrer(setting.Referrer)
                .WithCookies(cookieJar)
                .WithHeader("Accept-Encoding", null)
                .PostJsonAsync(new
                {
                    param = query,
                    page = page
                }, cancellationToken: cancellationToken)
                .ReceiveJson<List<DLCardEntryV2>>();

            foreach (var entry in cardEntryList)
            {
                (entry as DLCardEntry).CardType = ToCardType(entry.CardType);
                yield return entry;
            }

            page++;
        } while (cardEntryList?.Count > 29);

        Log.Information("Done Extracting. Total Pages: {pagenumber}", page);
    }

    private static int ToCardType(string cardTypeStr)
    {
        return cardTypeStr switch
        {
            "CH" => 2,// Character
            "EV" => 3,// Event
            "CX" => 4,// Climax
            _ => -1,// Unknown
        };
    }
}

internal class DLCardEntryV2 : DLCardEntry
{
    [JsonPropertyName("card_kind")]
    public new required string CardType { get; set; }
}