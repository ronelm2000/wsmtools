using Lamar;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.Cockatrice;
using Montage.Weiss.Tools.Utilities;
using System.Text.Json;
using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck;

public class CockatriceDeckParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    private readonly ILogger Log;
    private Func<CardDatabaseContext> _database;
    private System.Text.RegularExpressions.Regex serialParser = new System.Text.RegularExpressions.Regex(@"([^ ]*)(?: - )?(.*)(?:\n)?");

    public string[] Alias => new[] { "cockatrice", "cckt3s" };

    public int Priority => 1;

    public CockatriceDeckParser(IContainer container)
    {
        Log = Serilog.Log.ForContext<CockatriceDeckParser>();
        _database = () => container.GetInstance<CardDatabaseContext>();
        /*
        _parse = async (url) =>
        {
            var parser = container.GetInstance<ParseVerb>();
            parser.URI = url;
            await parser.Run(container);
        };
        */
    }

    public async Task<bool> IsCompatible(string urlOrFile)
    {
        var file = Fluent.IO.Path.Get(urlOrFile);
        await Task.CompletedTask;
        return file.Exists && file.Extension == ".cod";
    }

    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        Log.Information("Parsing: {source}", sourceUrlOrFile);
        var report = new DeckParserProgressReport
        {
            Percentage = 0,
            ReportMessage = new Card.API.Entities.Impls.MultiLanguageString
            {
                EN = $"Parsing Cockatrice Deck: {sourceUrlOrFile}"
            }
        };
        progress.Report(report);

        var file = Fluent.IO.Path.Get(sourceUrlOrFile);
        var serializer = new XmlSerializer(typeof(CockatriceDeck));
        using (var stream = file.GetStream())
        {
            // TODO: Copy code from DeckParser/DeckLogParser to get sets from EncoreDecks if they are missing.
            var nullCockatriceDeck = serializer.Deserialize(stream);
            if (nullCockatriceDeck is not CockatriceDeck cockatriceDeck)
                throw new DeckParsingException("Cannot parse into CockatriceDeck"); 
            var result = new WeissSchwarzDeck();
            var missingSerials = new List<String>();
            result.Name = cockatriceDeck.DeckName;
            result.Remarks = cockatriceDeck.Comments;
            result.Ratios = cockatriceDeck.Ratios.Ratios
                .Select(Translate) 
                .Select(p =>
                {
                    if (p.card == null)
                        missingSerials.Add(p.serial);
                    return p;
                })
                .Where(p => p.card != null) 
                .ToDictionary(p => p.card, p => p.amount);

            if (missingSerials.Count > 0)
            {
                throw new DeckParsingException($"The following cards are missing in the database. Please parse (or re-parse) them again. If you have any Promo Cards, you may need to parse a dedicated promo page first: {JsonSerializer.Serialize(missingSerials)} ");
            }
            else
            {
                report = new DeckParserProgressReport
                {
                    Percentage = 100,
                    ReportMessage = new Card.API.Entities.Impls.MultiLanguageString
                    {
                        EN = "Done parsing."
                    }
                };
                progress.Report(report);
                return await ValueTask.FromResult(result);
            }
        }
    }

    private (string serial, WeissSchwarzCard card, int amount) Translate(CockatriceSerialAmountPair pair)
    {
        var trueSerial = serialParser.Match(pair.Serial).Groups[1].Value;
        using (var db = _database())
        {
            return (trueSerial, db.WeissSchwarzCards.Find(trueSerial), pair.Amount);
        }
    }
}
