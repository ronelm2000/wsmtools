using Flurl.Http;
using Lamar;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Montage.Card.API.Entities.Impls;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck;

/// <summary>
/// Implements a Deck Parser that sources deck information from DeckLog.
/// Note that parsing the deck this way means the deck has no name or description, but the source link will be appended.
/// </summary>
public class DeckLogParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    //private Regex urlMatcher = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)");
    //private string deckLogApiUrlPrefix = "https://decklog.bushiroad.com/system/app/api/view/";
    private Dictionary<CardLanguage, DeckLogSettings> _settings = new()
    {
        [CardLanguage.English] = DeckLogSettings.English,
        [CardLanguage.Japanese] = DeckLogSettings.Japanese
    };

    private ILogger Log = Serilog.Log.ForContext<DeckLogParser>();
    private readonly Func<CardDatabaseContext> _database;
    private readonly Func<string, IProgress<CommandProgressReport>, CancellationToken, Task> _parse;

    public string[] Alias => new[] { "decklog" };

    public int Priority => 1;

    public DeckLogParser(IContainer ioc)
    {
        _database = () => ioc.GetInstance<CardDatabaseContext>();
        _parse = async (setGUID, progress, cancel) =>
        {
            var parser = ioc.GetInstance<ParseVerb>();
            parser.URI = $"https://www.encoredecks.com/api/series/{setGUID}/cards";
            await parser.Run(ioc, progress, cancel);
        };
    }

    public async Task<bool> IsCompatible(string urlOrFile)
    {
        await Task.CompletedTask;
        if (Uri.TryCreate(urlOrFile, UriKind.Absolute, out _))
        {
            return _settings.Any(s => s.Value.DeckURLMatcher.IsMatch(urlOrFile));
//                return urlMatcher.IsMatch(urlOrFile);
        }else
        {
            return false;
        }
    }

    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        var aggregator = new DeckLogParserAggregator(progress);
        aggregator.ReportParseStart(sourceUrlOrFile);

        var document = await sourceUrlOrFile.WithHTMLHeaders().GetHTMLAsync(cancellationToken);
        var (language, settings) = _settings.First(s => s.Value.DeckURLMatcher.IsMatch(sourceUrlOrFile));
        var deckID = settings.DeckURLMatcher.Match(sourceUrlOrFile).Groups[2];
        Log.Information("Parsing ID: {deckID}", deckID);
        var response = await $"{settings.DeckViewURL}{deckID}" //
            .WithReferrer(sourceUrlOrFile) //
            .PostJsonAsync(null, cancellationToken);
        var json = JsonConvert.DeserializeObject<dynamic>(await response.GetStringAsync());
        var newDeck = new WeissSchwarzDeck();
        var missingSerials = new List<string>();
        newDeck.Name = json.title.ToString();
        newDeck.Remarks = json.memo.ToString();

        aggregator.ReportParsedDeckData(newDeck);

        List<dynamic> items = new List<dynamic>();
        items.AddRange(json.list);
        items.AddRange(json.sub_list);

        using (var db = _database())
        {
            var missingSets = await items.Select(cardJSON => (string)(cardJSON.card_number.ToString()).Replace('＋', '+'))
                    .ToAsyncEnumerable()
                    .WhereAwaitWithCancellation(async (serial, ct) =>  (await db.WeissSchwarzCards.FindAsync(new[] { serial }, ct)) == null
                                                                    && (await db.WeissSchwarzCards.FindAsync(new[] { WeissSchwarzCard.RemoveFoil(serial) }, ct)) == null
                                               )
                    .Select(serial => WeissSchwarzCard.ParseSerial(serial).ReleaseID)
                    .Distinct()
                    .ToListAsync(cancellationToken);

            if (missingSets.Count > 0)
            {
                aggregator.ReportMissingSets(missingSets);
                Log.Warning("The following sets are missing from the database: @{sets}", missingSets);
                Log.Information("Parsing all missing sets from EncoreDecks. (Some sets may still be untranslated tho!)");
                var deckLogSetList = await "https://www.encoredecks.com/api/serieslist"
                    .WithRESTHeaders()
                    .GetJsonAsync<List<dynamic>>(cancellationToken);

                await deckLogSetList
                    .Where(set => missingSets.Contains($"{set.side}{set.release}"))
                    .Select(set => (string)set._id)
                    .ToAsyncEnumerable()
                    .ForEachAwaitWithCancellationAsync((set, ct) => _parse(set, aggregator, ct), cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var cardJSON in items)
            {
                string serial = cardJSON.card_number.ToString();
                serial = serial.Replace('＋', '+');
                if (serial == null)
                {
                    Log.Warning("serial is null for some reason!");
                }
                var card = await db.WeissSchwarzCards.FindAsync(new[] { serial }, cancellationToken);
                card = card ?? await WarnAndFindNonFoil(db, serial, cancellationToken);
                int quantity = cardJSON.num;
                if (card != null)
                {
                    Log.Debug("Adding: {card} [{quantity}]", card?.Serial, quantity);
                    if (newDeck.Ratios.TryGetValue(card, out int oldVal))
                        newDeck.Ratios[card] = oldVal + quantity;
                    else
                        newDeck.Ratios.Add(card, quantity);
                }
                else
                {
                    missingSerials.Add(serial);
                    //throw new DeckParsingException($"MISSING_SERIAL_{serial}");
                    Log.Debug("Serial has been effectively skipped because it's not found on the local db: [{serial}]", serial);
                }
            }
        }
        if (missingSerials.Count > 0)
            throw new DeckParsingException($"The following serials are missing from the DB:\n{missingSerials.ConcatAsString("\n")}");
        else
        {
            Log.Debug($"Result Deck: {JsonConvert.SerializeObject(newDeck.AsSimpleDictionary())}");
            aggregator.ReportSuccessfulParsedDeckData(newDeck);
            return newDeck;
        }
    }

    private async Task<WeissSchwarzCard> WarnAndFindNonFoil(CardDatabaseContext db, string serial, CancellationToken cancellationToken)
    {
        var nonFoilSerial = WeissSchwarzCard.RemoveFoil(serial);
        if (nonFoilSerial != serial)
        {
            Log.Warning("Unable to find {serial1}; trying to find {serial2} instead.", serial, nonFoilSerial);
            return await db.WeissSchwarzCards.FindAsync(new[] { nonFoilSerial }, cancellationToken);
        } else
        {
            return null;
        }
    }

    private class DeckLogParserAggregator : IProgress<CommandProgressReport>
    {
        private IProgress<DeckParserProgressReport> progress;
        private List<string> missingSets;
        internal DeckParserProgressReport report = new DeckParserProgressReport();

        public DeckLogParserAggregator(IProgress<DeckParserProgressReport> progress)
        {
            this.progress = progress;
        }

        internal void ReportParseStart(string url)
        {
            report = report with { Percentage = 0, ReportMessage = new MultiLanguageString { EN = $"Parsing DeckLog Deck: [{url}]" } };
            progress.Report(report);
        }

        internal void ReportParsedDeckData(WeissSchwarzDeck newDeck)
        {
            report = report with { Percentage = 10, ReportMessage = new MultiLanguageString { EN = $"Found Deck [{newDeck.Name}] [{newDeck.Remarks}]" } };
            progress.Report(report);
        }

        internal void ReportMissingSets(List<string> missingSets)
        {
            this.missingSets = missingSets;
            report = report with { Percentage = 10, ReportMessage = new MultiLanguageString { EN = $"Found Missing Sets; Parsing using EncoreDecks: [{missingSets.ConcatAsString(",")}" } };
            progress.Report(report);
        }

        public void Report(CommandProgressReport value)
        {
            // Only report if it's a message that says that the parsing progress is done?
            if (value.VerbType == CommandProgressReportVerbType.Parse && value.MessageType == MessageType.IsDone)
            {
                report = report with { Percentage = 10 + (int)(value.Percentage * 0.10f), ReportMessage = value.ReportMessage };
                progress.Report(report);
            }
        }

        internal void ReportSuccessfulParsedDeckData(WeissSchwarzDeck newDeck)
        {
            report = report.SuccessfullyParsedDeck(newDeck);
            progress.Report(report);
        }
    }
}
