using Flurl.Http;
using Lamar;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;

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
    private readonly Func<object, Task> _parse;

    public string[] Alias => new[] { "decklog" };

    public int Priority => 1;

    public DeckLogParser(IContainer ioc)
    {
        _database = () => ioc.GetInstance<CardDatabaseContext>();
        _parse = async (setGUID) =>
        {
            var parser = ioc.GetInstance<ParseVerb>();
            parser.URI = $"https://www.encoredecks.com/api/series/{setGUID}/cards";
            await parser.Run(ioc);
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

    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
    {
        var document = await sourceUrlOrFile.WithHTMLHeaders().GetHTMLAsync();
        var (language, settings) = _settings.First(s => s.Value.DeckURLMatcher.IsMatch(sourceUrlOrFile));
        var deckID = settings.DeckURLMatcher.Match(sourceUrlOrFile).Groups[2];
        Log.Information("Parsing ID: {deckID}", deckID);
        var response = await $"{settings.DeckViewURL}{deckID}" //
            .WithReferrer(sourceUrlOrFile) //
            .PostJsonAsync(null);
        var json = JsonConvert.DeserializeObject<dynamic>(await response.GetStringAsync());
        var newDeck = new WeissSchwarzDeck();
        var missingSerials = new List<string>();
        newDeck.Name = json.title.ToString();
        newDeck.Remarks = json.memo.ToString();

        List<dynamic> items = new List<dynamic>();
        items.AddRange(json.list);
        items.AddRange(json.sub_list);

        using (var db = _database())
        {
            var missingSets = await items.Select(cardJSON => (string)(cardJSON.card_number.ToString()).Replace('＋', '+'))
                    .ToAsyncEnumerable()
                    .WhereAwait(async (serial) => (await db.WeissSchwarzCards.FindAsync(serial)) == null
                                               && (await db.WeissSchwarzCards.FindAsync(WeissSchwarzCard.RemoveFoil(serial))) == null
                                                )
                    .Select(serial => WeissSchwarzCard.ParseSerial(serial).ReleaseID)
                    .Distinct()
                    .ToListAsync();

            if (missingSets.Count > 0)
            {
                Log.Warning("The following sets are missing from the database: @{sets}", missingSets);
                Log.Information("Parsing all missing sets from EncoreDecks. (Some sets may still be untranslated tho!)");
                var deckLogSetList = await "https://www.encoredecks.com/api/serieslist"
                    .WithRESTHeaders()
                    .GetJsonAsync<List<dynamic>>();

                await deckLogSetList
                    .Where(set => missingSets.Contains($"{set.side}{set.release}"))
                    .Select(set => (string)set._id)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(set => _parse(set));
            }

            foreach (var cardJSON in items)
            {
                string serial = cardJSON.card_number.ToString();
                serial = serial.Replace('＋', '+');
                if (serial == null)
                {
                    Log.Warning("serial is null for some reason!");
                }
                var card = await db.WeissSchwarzCards.FindAsync(serial);
                card = card ?? await WarnAndFindNonFoil(db, serial);
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
            return newDeck;
        }
    }

    private async Task<WeissSchwarzCard> WarnAndFindNonFoil(CardDatabaseContext db, string serial)
    {
        var nonFoilSerial = WeissSchwarzCard.RemoveFoil(serial);
        if (nonFoilSerial != serial)
        {
            Log.Warning("Unable to find {serial1}; trying to find {serial2} instead.", serial, nonFoilSerial);
            return await db.WeissSchwarzCards.FindAsync(nonFoilSerial);
        } else
        {
            return null;
        }
    }
}
