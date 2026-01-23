using Flurl.Http;
using Montage.Card.API.Entities.Impls;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

internal class RoseDeckLogClient : IDeckLogClient
{
    public async Task<bool> IsCompatible(WeissSchwarzCard card)
    {
        return await IsCompatible(card.Language, card.Side);
    }

    public async Task<bool> IsCompatible(CardLanguage language, CardSide side)
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
            KeywordQueryType = new[] { "no" }
        };
        var page = 1;
        var cookieJar = await cookieJarSrvc.FindOrCreate(setting.Authority);
        var cardEntryList = new List<DLCardEntryV2>();
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
                })
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

    private int ToCardType(string cardTypeStr)
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