using Fluent.IO;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Exceptions;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.JSON;
using Montage.Weiss.Tools.Utilities;
using System.Text.Json;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck;

public class LocalDeckJSONParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONParser>();
    public string[] Alias => new[] { "local", "json" };
    public int Priority => 1;
    private readonly Func<CardDatabaseContext> _database;

    public LocalDeckJSONParser(IContainer container)
    {
        _database = () => container.GetInstance<CardDatabaseContext>();
    }


    public async Task<bool> IsCompatible(string urlOrFile)
    {
        await Task.CompletedTask;
        var filePath = Path.Get(urlOrFile);
        if (!filePath.Exists)
            return false;
        else if (filePath.Extension != ".json")
            return false;
        else
            return true;
    }

    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        // TODO: Add DeckLog code here for getting missing sets.
        DeckParserProgressReport report = DeckParserProgressReport.AsStarting("Local Deck File");
        progress.Report(report);

        var filePath = Path.Get(sourceUrlOrFile);
        SimpleDeck deckJSON = JsonSerializer.Deserialize<SimpleDeck>(await filePath.ReadBytesAsync(cancellationToken)) ?? throw new DeckParsingException();
        WeissSchwarzDeck deck = new WeissSchwarzDeck();
        deck.Name = deckJSON.Name;
        deck.Remarks = deckJSON.Remarks;
        using (var db = _database())
        {
            await db.Database.MigrateAsync(cancellationToken);
            foreach (var serial in deckJSON.Ratios.Keys)
            {
                var card = await db.WeissSchwarzCards.FindAsync(new[] { serial }, cancellationToken);
                if (card == null)
                {
                    Log.Error("This card is missing in your local card db: {serial}", serial);
                    Log.Error("You must obtain information about this card first using the command {cmd}", "./wstools parse");
                    return WeissSchwarzDeck.Empty;
                }
                else
                {
                    deck.Ratios[card] = deckJSON.Ratios[serial];
                }
            }
        }

        report = report.SuccessfullyParsedDeck(deck);
        progress.Report(report);

        return deck;
    }
}
