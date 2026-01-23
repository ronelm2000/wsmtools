using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public partial class DeckLogPostProcessor : ICardPostProcessor<WeissSchwarzCard>, ISkippable<IParseInfo>
{
    private ILogger Log = Serilog.Log.ForContext<DeckLogPostProcessor>();

    private readonly Func<CardDatabaseContext> _db;
    private readonly Func<GlobalCookieJar> _cookieJar;
    private readonly Func<ICachedMapService<(CardLanguage,string), Dictionary<string, DLCardEntry>>> _cacheSrvc;

    private static readonly IDeckLogClient[] deckLogClients = new IDeckLogClient[]
    {
        new OriginalDeckLogClient(),
        new RoseDeckLogClient()
    };

    private string? currentVersion;

    private bool isOutdated = false;

    public int Priority => 1;

    public DeckLogPostProcessor(IContainer ioc)
    {
        _db = () => ioc.GetInstance<CardDatabaseContext>();
        _cookieJar = () => ioc.GetInstance<GlobalCookieJar>();
        _cacheSrvc = () => ioc.GetInstance<ICachedMapService<(CardLanguage,string), Dictionary<string, DLCardEntry>>>();
    }

    public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
    {
        var firstCard = cards[0];
        var deckLogClient = await deckLogClients.ToAsyncEnumerable().FirstOrDefaultAwaitAsync(async c => await c.IsCompatible(firstCard));
        if (deckLogClient is null)
            return false;

        var settings = FindCorrectConfig(cards[0]);
        if (settings is null)
            return false;

        var latestVersion = await GetLatestVersion(settings);
        if (latestVersion != settings.Version)
        {
            Log.Warning("DeckLog's API has been updated from {version1} to {version2}.", settings.Version, latestVersion);
            Log.Warning("Please check with the developer for a newer version that ensures compatibility with the newest version.");
            isOutdated = true;
        }
        return true;
    }

    public async Task<string> GetLatestVersion(DeckLogSettings settings)
    {
        var cookieJar = await _cookieJar().FindOrCreate(settings.Referrer);
        currentVersion ??= await settings.VersionURL
            .WithRESTHeaders()
            .WithCookies(cookieJar)
            .AfterCall(c =>
            {
                if (c.Response.Headers.TryGetFirst("Content-Encoding", out var encoding))
                {
                    Log.Information("Response is encoded with {encoding}.", encoding);
                }
            })
            .GetStringAsync();

        return currentVersion;
    }

    public async Task<bool> IsIncluded(IParseInfo info)
    {
        await Task.CompletedTask;
        if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:decklog"))
        {
            Log.Information("Skipping due to the parser hint [skip:decklog].");
            return false;
        }
        else if (info.ParserHints.Contains("skip:external", StringComparer.CurrentCultureIgnoreCase))
        {
            Log.Information("Skipping due to parser hint [skip:external].");
            return false;
        }

        if (isOutdated)
        {
            if (info.ParserHints.Contains("strict", StringComparer.CurrentCultureIgnoreCase))
            {
                Log.Information("Not executing due to [strict] flag.");
                return false;
            }
            else
            {
                Log.Warning("DeckLog is now enabled by default, but expect bugs due to version incompability.");
                return true;
            }
        }
        else
        {
            return true;
        }
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards, IProgress<PostProcessorProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cardData = await originalCards.ToListAsync(cancellationToken);
        List<CardLanguage> languages = cardData.Select(c => c.Language).Distinct().ToList();
        var settings = FindCorrectConfig(cardData[0])!;
        Log.Information("Starting...");
        var titleCodes = cardData.Select(c => c.TitleCode).Distinct().ToArray();
        var deckLogSearchResults = await GetDeckLogSearchResults(cardData, settings, cancellationToken);
        var newPRCards = Enumerable.Empty<WeissSchwarzCard>().ToAsyncEnumerable();
        using (var db = _db())
        {
            var prCards = db.WeissSchwarzCards.AsAsyncEnumerable()
                .Where(c => titleCodes.Contains(c.TitleCode)
                            && c.Language == CardLanguage.Japanese
                            && c.Rarity == "PR"
                            && !c.Images.Any(u => u.AbsoluteUri.StartsWith(settings.ImagePrefix))
                      )
                .Select(c => db.Attach(c).Entity);

            Log.Information("Post-Processing PRs...");
            await foreach (var card in prCards)
                newPRCards = newPRCards.Concat(TryMutate(card, deckLogSearchResults, settings, cancellationToken));

            var results = await db.SaveChangesAsync();
            Log.Information("Changed: {results} rows.", results);
        }

        var cardList = cardData.ToAsyncEnumerable()
            .SelectMany(c => TryMutate(c, deckLogSearchResults, settings, cancellationToken))
            .Concat(newPRCards)
            .WithCancellation(cancellationToken);

        await foreach (var card in cardList)
            yield return card;
    }

    private async IAsyncEnumerable<WeissSchwarzCard> TryMutate(WeissSchwarzCard originalCard, IDictionary<string, DLCardEntry> deckLogSearchData, DeckLogSettings settings, [EnumeratorCancellation] CancellationToken token = default)
    {
        if (!deckLogSearchData.ContainsKey(originalCard.Serial + originalCard.Rarity))
        {
            Log.Warning("Unable to find data for [{serial}], no image data was extracted...", originalCard.Serial);
            yield break;
        }
        Log.Information("Found and adding data: [{card}]", originalCard.Serial);
        var deckLogData = deckLogSearchData[originalCard.Serial + originalCard.Rarity];
        originalCard.Name.JP = deckLogData.Name;
        originalCard.Images.Add(new Uri($"{settings.ImagePrefix}{deckLogData.ImagePath}"));
        yield return originalCard;

        var foilDeckLogList = deckLogSearchData.Keys
            .ToAsyncEnumerable()
            .Where(k => k.Contains(originalCard.Serial) && (originalCard.Serial + originalCard.Rarity != k))
            .SelectParallelAsync(async k => await ValueTask.FromResult(deckLogSearchData[k]), cancellationToken: token);

        await foreach (var foilDLData in foilDeckLogList)
        {
            var newFoilCard = originalCard.Clone();
            newFoilCard.Name.JP = foilDLData.Name;
            newFoilCard.Images.Clear();
            newFoilCard.Images.Add(new Uri($"{settings.ImagePrefix}{foilDLData.ImagePath}"));
            newFoilCard.Serial = foilDLData?.Serial ?? throw new NullReferenceException();
            newFoilCard.Rarity = foilDLData?.Rarity ?? throw new NullReferenceException();
            Log.Information("Adding Foil: {serial} [{rarity}]", newFoilCard.Serial, newFoilCard.Rarity);
            yield return newFoilCard;
        }
    }

    private async Task<IDictionary<string, DLCardEntry>> GetDeckLogSearchResults(List<WeissSchwarzCard> cardData, DeckLogSettings settings, CancellationToken cancellationToken)
    {
        var cacheSrvc = _cacheSrvc();
        var titleCodes = cardData.Select(c => c.TitleCode).ToHashSet();
        var results = new Dictionary<string, DLCardEntry>();
        var cacheResults = cacheSrvc.GetValues(titleCodes.Select(t => (settings.Language, t)))
            .Where(c => c.Value.Count > 0)
            .ToDictionary(c => c.Key, c => c.Value);
        var cacheValues = cacheResults.SelectMany(c => c.Value).ToList();
        var serialMapper = cardData.ToDictionary(c => c.Serial, c => c.TitleCode);
        var cookieJar = await _cookieJar().FindOrCreate(settings.Referrer);

        foreach (var kvp in cacheValues)
            results.Add(kvp.Key, kvp.Value);

        if (titleCodes.Count < 10) //TODO: How do we indicate that this is a WPR extraction? 
            titleCodes.RemoveWhere(t => cacheResults.ContainsKey((settings.Language, t)));

        if (titleCodes.Count < 1)
            return results;

        var firstCard = cardData[0];
        var deckLogClient = await deckLogClients.ToAsyncEnumerable().FirstOrDefaultAwaitAsync(async c => await c.IsCompatible(firstCard), cancellationToken);

        foreach (var titleCode in titleCodes)
        {
            Log.Information("Fetching DeckLog Data for [{titlecode}]...", titleCode);
            var deckLogContext = new DeckLogContext
            {
                CookieJar = _cookieJar(),
                CacheService = cacheSrvc,
                Authority = settings.Authority,
                Language = settings.Language
            };
            await foreach (var entry in deckLogClient!.FindCardEntries(deckLogContext, titleCode, cancellationToken))
            {
                if (entry.Serial is null || entry.Rarity is null)
                {
                    Log.Warning("Detected unusual output: {@entry}", entry);
                    continue;
                }
                results[entry.Serial + entry.Rarity] = entry;
                var serialEncoded = WeissSchwarzCard.ParseSerial(entry.Serial);
                cacheSrvc[(settings.Language, serialEncoded.NeoStandardCode)][entry.Serial + entry.Rarity] = entry;
                Log.Debug("Encoded DeckLog Result: {serial}", entry.Serial + entry.Rarity);
            }

        }
        return results;
    }

    private DeckLogSettings? FindCorrectConfig(WeissSchwarzCard firstCard)
    {
        return firstCard switch
        {
            var c when c.Side == CardSide.Rose => DeckLogSettings.Rose,
            var c when c.Language == CardLanguage.Japanese => DeckLogSettings.Japanese,
            var c when c.Language == CardLanguage.English => DeckLogSettings.English,
            _ => null
        };
    }
}
