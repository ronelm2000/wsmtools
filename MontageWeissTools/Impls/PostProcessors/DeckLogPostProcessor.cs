using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.PostProcessors
{
    public class DeckLogPostProcessor : ICardPostProcessor, ISkippable<IParseInfo>
    {
        private ILogger Log = Serilog.Log.ForContext<DeckLogPostProcessor>();

        private DeckLogSettings settings = new DeckLogSettings();

//        private string defaultAwsWeissSchwarzSitePrefix = "https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/";
//        private string defaultReferrer = "https://decklog.bushiroad.com/create?c=2";
//        private string defaultRESTSearchURL = "https://decklog.bushiroad.com/system/app/api/search/2";
//        private string defaultRESTCardParamURL = "";
       // private readonly Func<CardDatabaseContext> _db;
        private readonly Func<Task<string>> _getLatestVersion;

        private string currentVersion;

        public int Priority => 1;

        public DeckLogPostProcessor(IContainer ioc)
        {
            //_db = () => ioc.GetInstance<CardDatabaseContext>();
            _getLatestVersion = async () =>
            {              
                currentVersion = currentVersion ?? await settings.VersionURL.GetStringAsync();
                return currentVersion;
            };
        }

        public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
        {
            if (cards.Any(c => c.Language == CardLanguage.English))
                return false;
            if ((await _getLatestVersion()) != settings.Version)
            {
                Log.Warning("DeckLog's API has been updated from {version1} to {version2}.", settings.Version, await _getLatestVersion());
                Log.Warning("Please check with the developer for a newer version that ensures compatibility with the newest version.");
            }
            return true;
        }

        public async Task<bool> IsIncluded(IParseInfo info)
        {
            await Task.CompletedTask;
            if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:decklog"))
            {
                Log.Information("Skipping due to the parser hint [skip:decklog].");
                return false;
            }  if ((await _getLatestVersion()) != settings.Version)
            {
                if (info.ParserHints.Contains("nowarn", StringComparer.CurrentCultureIgnoreCase))
                {
                    Log.Warning("Executing due to [nowarn] flag. Expect bugs due to version incompability.");
                    return true;
                }
                else
                {
                    Log.Information("Skipping.");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards)
        {
            var cardData = await originalCards.ToListAsync();
            Log.Information("Starting...");
            var titleCodes = cardData.Select(c => c.TitleCode).Distinct().ToArray();
            var deckLogSearchResults = await GetDeckLogSearchResults(cardData);
            /*
            Log.Information("Searching for PR cards already in database (if any)...");
            using (var db = _db())
            {
                var query = db.WeissSchwarzCards.AsQueryable();
                foreach (var titleCode in titleCodes)
                    query = query.Where(c => c.Serial.StartsWith(titleCode));
                await foreach (var card in query.Where(c => c.Rarity == "PR").ToAsyncEnumerable())
                    db.Update(TryMutate(card, deckLogSearchResults));
                await db.SaveChangesAsync();
            }
            */

            foreach (var card in cardData)
                yield return TryMutate(card, deckLogSearchResults);
        }

        private WeissSchwarzCard TryMutate(WeissSchwarzCard originalCard, Dictionary<string, DLCardEntry> deckLogSearchData)
        {
            if (deckLogSearchData.ContainsKey(originalCard.Serial + originalCard.Rarity))
            {
                Log.Information("Found and adding data: [{card}]", originalCard.Serial);
                var deckLogData = deckLogSearchData[originalCard.Serial + originalCard.Rarity];
                originalCard.Name.JP = deckLogData.Name;
                originalCard.Images.Add(new Uri($"{settings.ImagePrefix}{deckLogData.ImagePath}"));
            }
            else
            {
                Log.Warning("Unable to find data for [{serial}], no image data was extracted...", originalCard.Serial);
            }
            return originalCard;
        }
        private async Task<Dictionary<string, DLCardEntry>> GetDeckLogSearchResults(List<WeissSchwarzCard> cardData)
        {
            var results = new Dictionary<string, DLCardEntry>();
            List<DLCardEntry> temporaryResults = null;
            // var queryData = GenerateSearchJSON(titleCodes);
            //Log.Information($"Accessing DeckLog API with the following query data: {JsonConvert.SerializeObject(queryData, Formatting.Indented)}");
            var cardParams = await settings.CardParamURL
                .WithRESTHeaders()
                .WithReferrer(settings.Referrer)
                .PostJsonAsync(new { })
                .ReceiveJson<DLQueryParameters>();
            var titleCodes = cardData.Select(c => c.TitleCode).ToHashSet();
            IEnumerable<DLCardQuery> queries = GetCardQueries(cardData, cardParams, titleCodes);
            foreach (var queryData in queries)
            {
                int page = 1;
                Log.Information($"Accessing DeckLog API with the following query data: {JsonConvert.SerializeObject(queryData, Formatting.Indented)}");
                do
                {
                    Log.Information("Extracting Page {pagenumber}...", page);
                    temporaryResults = await settings.SearchURL.WithRESTHeaders()
                        .WithReferrer(settings.Referrer)
                        .PostJsonAsync(new
                        {
                            param = queryData,
                            page = page
                        })
                        .ReceiveJson<List<DLCardEntry>>();
                    foreach (var entry in temporaryResults)
                        results.Add(entry.Serial + entry.Rarity, entry);
                    Log.Information("Got {count} results...", temporaryResults?.Count ?? 0);
                    page++;
                } while (temporaryResults.Count > 29);
            }
            return results;
        }

        private IEnumerable<DLCardQuery> GetCardQueries(List<WeissSchwarzCard> cardData, DLQueryParameters cardParams, HashSet<string> titleCodes)
        {
            //var matchesOneNSTitle = cardParams.GetTitleSelectionKeys().Any(a => titleCodes.Except(a).SequenceEqual(Array.Empty<string>()));
            var titles = cardParams.GetTitleSelectionKeys().Where(a => titleCodes.Intersect(a).Count() > 0).Select(a => titleCodes.Intersect(a).ToArray()).ToArray();
            Log.Information("Neo-Standard Titles Found: {count}", titles.Length);
            Log.Information("All Titles: {@titles}", titles);
            Log.Information("Removing Sub-Lists...");
            titles = titles.Where(t1 => titles.All(t2 => t1 == t2 || !t1.Union(t2).ToHashSet().SetEquals(t2))).ToArray();
            Log.Information("Neo-Standard Titles Found: {count}", titles.Length);
            Log.Information("All Titles: {@titles}", titles);
            if (titles.Length < 5)
                return GenerateSearchJSON(titles.SelectMany(t => t).Distinct());
            else
                return GenerateSearchJSON(cardData);
        }

        private IEnumerable<DLCardQuery> GenerateSearchJSON(IEnumerable<string> titleCodes)
        {
            yield return new DLCardQuery
            {
                Titles = $"##{titleCodes.ConcatAsString("##")}##"
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
            [JsonProperty("title_number_select_for_search")]
            public Dictionary<string, TitleSelection> TitleSelectionsForSearch { get; set; }

            public IEnumerable<string[]> GetTitleSelectionKeys()
            {
                return TitleSelectionsForSearch.Keys.Select(s => s.Split("##", StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private class TitleSelection
        {
            public int Side { get; set; } // -1 = Weiss ; -2 = Schwarz ; -3 = Both
            public string Label { get; set; }
            public int ID { get; set; }
        }

        private class DLCardEntry
        {
            [JsonProperty("card_number")]
            public string Serial { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("rare")]
            public string Rarity { get; set; }  
            [JsonProperty("img")]
            public string ImagePath { get; set; }
        }

        private class DLCardQuery
        {
            [JsonProperty("title_number")]
            public string Titles { get; set; } = "";
            [JsonProperty("keyword")]
            public string Keyword { get; set; } = "";
            [JsonProperty("keyword_type")]
            public string[] KeywordQueryType { get; set; } = new string[] { "no" };
            [JsonProperty("side")]
            public string Side { get; set; } = "";
            [JsonProperty("card_kind")]
            public CardType TypeQuery { get; set; } = CardType.All;
            [JsonProperty("color")]
            public CardColor ColorQuery { get; set; } = CardColor.All;
            [JsonProperty("option_clock")]
            public bool CounterCardsOnly { get; set; } = false;
            [JsonProperty("option_counter")]
            public bool ClockCardsOnly { get; set; } = false;
            [JsonProperty("deck_param1")]
            public DeckConstructionType DeckConstruction { get; set; } = DeckConstructionType.Standard;
            [JsonProperty("deck_param2")]
            public string DeckConstructionParameter { get; set; } = "";

            //TODO: There's actually alot of missing variables that can be placed here, but these are ignored for now.
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataContract]
        private enum CardType {
            [EnumMember(Value = "0")]
            All,
            [EnumMember(Value = "2")]
            Character,
            [EnumMember(Value = "3")]
            Event,
            [EnumMember(Value = "4")]
            Climax
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataContract]
        private enum CardColor
        {
            [EnumMember(Value = "0")]
            All,
            [EnumMember(Value = "yellow")]
            Yellow,
            [EnumMember(Value = "green")]
            Green,
            [EnumMember(Value = "red")]
            Red,
            [EnumMember(Value = "blue")]
            Blue
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataContract]
        private enum DeckConstructionType
        {
            [EnumMember(Value = "S")]
            Standard,
            [EnumMember(Value = "N")]
            NeoStandard,
            [EnumMember(Value = "T")]
            TitleOnly,
            [EnumMember(Value = "O")]
            Others
        }

        private class DeckLogSettings
        {
            public string Version { get; set; } = "20201113.001";
            public string VersionURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/version/";
            public string ImagePrefix { get; set; } = "https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/";
            public string Referrer { get; set; } = "https://decklog.bushiroad.com/create?c=2";
            public string SearchURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/search/2";
            public string CardParamURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/cardparam/2";
        }
    }
}
