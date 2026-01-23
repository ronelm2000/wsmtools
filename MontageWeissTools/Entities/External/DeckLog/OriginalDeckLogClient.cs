using Flurl.Http;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

internal class OriginalDeckLogClient : IDeckLogClient
{
    private static Dictionary<(string, CardLanguage), DeckLogSettings> Presets { get; } = new DeckLogSettings[] {
        DeckLogSettings.Japanese,
        DeckLogSettings.English,
        DeckLogSettings.JapaneseInEN
    }.ToDictionary(c => (c.Authority, c.Language), c => c);

    private static Dictionary<CardLanguage, Dictionary<string, string>> cachedNeoStandardList = new();

    public async Task<bool> IsCompatible(WeissSchwarzCard card)
    {
        return await IsCompatible(card.Language, card.Side);
    }

    public async Task<bool> IsCompatible(CardLanguage language, CardSide side)
    {
        return side switch
        {
            CardSide.Schwarz => true,
            CardSide.Weiss => true,
            CardSide.Both => true,
            _ => false
        };
    }

    public async IAsyncEnumerable<DLCardEntry> FindCardEntries(DeckLogContext context, string nsCode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (language, authority, cacheSrvc, _cookieJar) = context;
        var setting = Presets[(authority, language)];
        var cardEntries = Expand(setting, nsCode)
            .Select(ToQuery)
            .SelectMany(q => ExecuteQuery(setting, q))
            .WithCancellation(cancellationToken);

        await foreach (var entry in cardEntries)
            yield return entry;
    }

    private async IAsyncEnumerable<string> Expand(DeckLogSettings setting, string nsCode)
    {
        var cardParams = (cachedNeoStandardList.ContainsKey(setting.Language)) ? cachedNeoStandardList[setting.Language] : await setting.SuggestURL
            .WithReferrer(setting.Referrer)
            .WithRESTHeaders()
            .WithHeader("Accept-Encoding", null)
            .BeforeCall(c =>
            {
                Log.Debug("Request: {url} || {headers}", c.Request.Url, c.Request.Headers.Select(e => $"[{e.Name}, {e.Value}]").ConcatAsString(" "));
                Log.Debug("Body: {body}", c.RequestBody);
            })
            .AfterCall(async c =>
            {
                var rawContent = await c.Response.ResponseMessage.Content.ReadAsStringAsync();
                Log.Debug("Response Content-Encoding: {encoding}", c.Response.Headers.TryGetFirst("Content-Encoding", out var encoding) ? encoding : "none");
                Log.Debug("Response: {response}", rawContent);
            })
            .PostJsonAsync(new { Param = "" })
            .ReceiveJson<Dictionary<string, string>>();

        var titleCodes = cardParams.Keys
            .Select(s => (s, s.Split("##", StringSplitOptions.RemoveEmptyEntries)))
            .Where(p => p.Item2.Contains(nsCode))
            .Select(p => p.s)
            .ToArray();
        
        foreach (var titleCode in titleCodes)
            yield return titleCode;
    }

    private DLCardQuery ToQuery(string nsKey) {
        return new DLCardQuery
            {
                DeckConstruction = DeckConstructionType.NeoStandard,
                DeckConstructionParameter = nsKey
            };
    }

    private async IAsyncEnumerable<DLCardEntry> ExecuteQuery(DeckLogSettings setting, DLCardQuery query)
    {
        int page = 1;
        Log.Information($"Accessing DeckLog API with the following query data: {JsonSerializer.Serialize(query, new JsonSerializerOptions { WriteIndented = true })}");
        var cardEntryList = new List<DLCardEntry>();
        do
        {
            Log.Information("Extracting Page {pagenumber}...", page);
            cardEntryList = await setting.SearchURL
                .WithReferrer(setting.Referrer)
                .WithRESTHeaders()
                .PostJsonAsync(new
                {
                    param = query,
                    page = page
                })
                .ReceiveJson<List<DLCardEntry>>();

            foreach (var entry in cardEntryList)
                yield return entry;

            page++;
        } while (cardEntryList?.Count > 29);
    }
}
