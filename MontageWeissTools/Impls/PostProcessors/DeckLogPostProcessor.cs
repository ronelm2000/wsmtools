using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public partial class DeckLogPostProcessor : ICardPostProcessor<WeissSchwarzCard>, ISkippable<IParseInfo>
{
    private ILogger Log = Serilog.Log.ForContext<DeckLogPostProcessor>();

    private readonly Func<CardDatabaseContext> _db;
    private readonly Func<GlobalCookieJar> _cookieJar;
    private readonly Func<ICachedMapService<(CardLanguage,string), Dictionary<string, DLCardEntry>>> _cacheSrvc;

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
        List<CardLanguage> languages = cards.Select(c => c.Language).Distinct().ToList();
        if (languages.Count != 1) {
            return false;
        }
        if (languages[0] == CardLanguage.English && cards[0].EnglishSetType == EnglishSetType.Custom)
            return false;
        var settings = (languages[0] == CardLanguage.English) ? DeckLogSettings.English : DeckLogSettings.Japanese;
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
        return currentVersion ?? (currentVersion = await settings.VersionURL.WithCookies(_cookieJar()[settings.Referrer]).GetStringAsync());
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
        var settings = (languages[0] == CardLanguage.English) ? DeckLogSettings.English : DeckLogSettings.Japanese;
        Log.Information("Starting...");
        var titleCodes = cardData.Select(c => c.TitleCode).Distinct().ToArray();
        var deckLogSearchResults = await GetDeckLogSearchResults(cardData, settings);
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
    
    private async Task<IDictionary<string, DLCardEntry>> GetDeckLogSearchResults(List<WeissSchwarzCard> cardData, DeckLogSettings settings)
    {
        var cacheSrvc = _cacheSrvc();
        var titleCodes = cardData.Select(c => c.TitleCode).ToHashSet();
        var results = new Dictionary<string, DLCardEntry>();
        var cacheResults = cacheSrvc.GetValues(titleCodes.Select(t => (settings.Language, t)))
            .Where(c => c.Value.Count > 0)
            .ToDictionary(c => c.Key, c => c.Value);
        var cacheValues = cacheResults.SelectMany(c => c.Value).ToList();
        var serialMapper = cardData.ToDictionary(c => c.Serial, c => c.TitleCode);

        foreach (var kvp in cacheValues)
            results.Add(kvp.Key, kvp.Value);

        if (titleCodes.Count < 10) //TODO: How do we indicate that this is a WPR extraction? 
            titleCodes.RemoveWhere(t => cacheResults.ContainsKey((settings.Language,t)));

        if (titleCodes.Count < 1)
            return results;

        List<DLCardEntry> temporaryResults = null!;
        var cardParams = await settings.SuggestURL
            .WithRESTHeaders()
            .WithReferrer(settings.Referrer)
            .WithCookies(_cookieJar()[settings.Referrer])
            .PostJsonAsync(new { Param = "" })
            .ReceiveJson<Dictionary<string,string>>();

        IEnumerable<DLCardQuery> queries = GetCardQueries(cardData, cardParams, titleCodes);
        foreach (var queryData in queries)
        {
            int page = 1;
            Log.Information($"Accessing DeckLog API with the following query data: {JsonSerializer.Serialize(queryData, new JsonSerializerOptions { WriteIndented = true })}");
            do
            {
                Log.Information("Extracting Page {pagenumber}...", page);
                temporaryResults = await settings.SearchURL.WithRESTHeaders()
                    .WithReferrer(settings.Referrer)
                    .WithCookies(_cookieJar()[settings.Referrer])
                    .PostJsonAsync(new
                    {
                        param = queryData,
                        page = page
                    })
                    .ReceiveJson<List<DLCardEntry>>();
                foreach (DLCardEntry entry in temporaryResults)
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
                Log.Information("Got {count} results...", temporaryResults?.Count ?? 0);
                page++;
            } while (temporaryResults?.Count > 29);
        }
        return results;
    }

    private IEnumerable<DLCardQuery> GetCardQueries(List<WeissSchwarzCard> cardData, Dictionary<string,string> cardParams, HashSet<string> titleCodes)
    {
        var titles = cardParams.Keys
            .Select(s => (s, s.Split("##", StringSplitOptions.RemoveEmptyEntries)))
            .Where(p => titleCodes.Any(t => p.Item2.Contains(t))) // p.Item2.Intersect(titleCodes).Count() == p.Item2.Length)
            .Select(p => p.s)
            .ToArray();

        if ((titles?.Length ?? 0) < 1)
            titles = cardParams.Keys
                .Select(s => (s, s.Split("##", StringSplitOptions.RemoveEmptyEntries)))
                .Where(p => p.Item2.Intersect(titleCodes).Count() == titleCodes.Count)
                .Select(p => p.s)
                .ToArray();

        //        var titles = cardParams.Keys
        //            .Select(s => s.Split("##", StringSplitOptions.RemoveEmptyEntries))
        //            .Where(a => titleCodes.Intersect(a).Count() > 0)
        //            .Select(a => titleCodes.Intersect(a).ToArray())
        //            .ToArray();
        //        Log.Information("Neo-Standard Titles Found: {count}", titles.Length);
        //       Log.Information("All Titles: {@titles}", titles);
        //       Log.Information("Removing duplicate sub-lists...");
        //      titles = titles.DistinctBy(sa => sa.GetHashCode()).ToArray();
        //      Log.Information("Removing subsets...");
        //     var duplicateTitles = titles.Where(title => titles.Any(tsuperset => title.ToHashSet().IsProperSubsetOf(tsuperset))).ToArray();
        //    titles = titles.Except(duplicateTitles).ToArray();
        Log.Information("Neo-Standard Titles Found: {count}", titles?.Length ?? 0);
        Log.Information("All Titles: {@titles}", titles);
        if ((titles?.Length ?? 0) < 5)
            return GenerateSearchJSONV2(titles!);
        else
            return GenerateSearchJSON(cardData);
    }

    private IEnumerable<DLCardQuery> GenerateSearchJSONV2(IEnumerable<string> nsKeys)
    {
        foreach (var nsKey in nsKeys)
            yield return new DLCardQuery
            {
                DeckConstruction = DeckConstructionType.NeoStandard,
                DeckConstructionParameter = nsKey
            };
    }
    private IEnumerable<DLCardQuery> GenerateSearchJSON(IEnumerable<string> titleCodes)
    {
        yield return new DLCardQuery
        {
            DeckConstruction = DeckConstructionType.NeoStandard,
            DeckConstructionParameter = $"##{titleCodes.ConcatAsString("##")}##"
        };
    }

    private IEnumerable<DLCardQuery> GenerateSearchJSON(List<WeissSchwarzCard> cardData)
    {
        foreach (var card in cardData) {
            yield return new DLCardQuery
            {
                Keyword = card.Serial
            };
        }
    }

    private class DLQueryParameters
    {
        [JsonPropertyName("title_number_select_for_search")]
        public Dictionary<string, TitleSelection> TitleSelectionsForSearch { get; set; } = new();

        public IEnumerable<string[]> GetTitleSelectionKeys()
        {
            return TitleSelectionsForSearch.Keys.Select(s => s.Split("##", StringSplitOptions.RemoveEmptyEntries));
        }
    }

    private class TitleSelection
    {
        public int Side { get; set; } // -1 = Weiss ; -2 = Schwarz ; -3 = Both
        public string Label { get; set; } = string.Empty;
        public int ID { get; set; }
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

    private class DLCardQuery
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
    private enum CardType {
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
    private enum CardColor
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
    private enum DeckConstructionType
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
}
