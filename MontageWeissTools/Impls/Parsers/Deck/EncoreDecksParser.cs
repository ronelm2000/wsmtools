using Flurl;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.IO;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck;

public class EncoreDecksParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    private ILogger Log { get; }

    // Dependencies
    private readonly Func<CardDatabaseContext> _database;
    private readonly Func<string, IProgress<CommandProgressReport>, CancellationToken, Task> _parse;

    public string[] Alias => new[] { "encoredecks", "ed" };
    public int Priority => 1;

    public EncoreDecksParser(IContainer container)
    {
        Log = Serilog.Log.ForContext<EncoreDecksParser>();
        _database = () => container.GetInstance<CardDatabaseContext>();
        _parse = async (url, p, ct) =>
        {
            var parser = container.GetInstance<ParseVerb>();
            parser.URI = url;
            await parser.Run(container, p, ct);
        };
    }


    public async Task<bool> IsCompatible(string urlOrFile)
    {
        await Task.CompletedTask;
        if (urlOrFile == null) return false;

        return (File.Exists(urlOrFile) && urlOrFile.EndsWith(".csv")) ||
            urlOrFile.StartsWith("https://www.encoredecks.com/deck/") ||
            urlOrFile.StartsWith("http://www.encoredecks.com/deck/");
    }

    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        if (File.Exists(sourceUrlOrFile))
            return await ParseFromCSV(sourceUrlOrFile, progress, cancellationToken);
        else
            return await Parse(new Uri(sourceUrlOrFile), progress, cancellationToken);
    }

    private async Task<WeissSchwarzDeck> ParseFromCSV(string sourceCSV, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken)
    {
        WeissSchwarzDeck res = new WeissSchwarzDeck();
        using (var db = _database())
            foreach (var row in ParseCSV(sourceCSV, b => b.SetDelimiters(",")))
            {
                if (row[0] == "Code") continue;
                var card = await db.WeissSchwarzCards.FindAsync(row[0]);
                var quantity = row[1].AsParsed<int>(int.TryParse).GetValueOrDefault(0);
                res.Ratios.Add(card, quantity);
            }

        res.Remarks = (res.Remarks ?? "") + $"\nParsed: {this.GetType().Name}";
        res.Remarks = res.Remarks.Trim();
        return res;
    }

    private async Task<WeissSchwarzDeck> Parse(Uri uri, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken)
    {
        var report = DeckParserProgressReport.AsStarting("EncoreDecks");
        progress.Report(report);

        var encoreDecksDeckAPIURL = "https://www.encoredecks.com/api/deck";
        var localPath = uri.LocalPath;
        var deckID = localPath.Substring(localPath.LastIndexOf('/') + 1);
        Log.Information("Deck ID: {deckID}", deckID);

        dynamic deckJSON = await GetDeckJSON(encoreDecksDeckAPIURL, deckID, cancellationToken);

        WeissSchwarzDeck res = new WeissSchwarzDeck();
        res.Name = deckJSON.name;

        report = report.ObtainedParseDeckData(res);
        progress.Report(report);

        var parseTranslator = progress.From().Translate<CommandProgressReport>(TranslateProgress);

        using (var db = _database())
        {
            await db.Database.MigrateAsync(cancellationToken);

            foreach (dynamic card in deckJSON.cards)
            {
                string serial = WeissSchwarzCard.GetSerial(card.set.ToString(), card.side.ToString(), card.lang.ToString(), card.release.ToString(), card.sid.ToString());
                WeissSchwarzCard wscard = await db.WeissSchwarzCards.FindAsync(serial);
                if (wscard == null)
                {
                    string setID = card.series;
                    await _parse($"https://www.encoredecks.com/api/series/{setID}/cards", parseTranslator, cancellationToken);
                    wscard = await db.WeissSchwarzCards.FindAsync(new[] { serial }, cancellationToken);
                }

                if (res.Ratios.TryGetValue(wscard, out int quantity))
                    res.Ratios[wscard]++;
                else
                    res.Ratios[wscard] = 1;
            }
        }
        var simpleRatios = res.AsSimpleDictionary();
        Log.Information("Deck Parsed: {@simpleRatios}", simpleRatios);
        Log.Information("Cards in Deck: {@count}", simpleRatios.Values.Sum());

        report = report.SuccessfullyParsedDeck(res);
        progress.Report(report);

        return res;
    }

    async Task<dynamic> GetDeckJSON(string encoreDecksDeckAPIURL, string deckID, CancellationToken ct = default)
    {
        return await encoreDecksDeckAPIURL
                        .AppendPathSegment(deckID)
                        .WithRESTHeaders()
                        .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                        .WithHeader("Accept", "text/plain")
                        .GetJsonAsync<dynamic>(ct);
    }

    private IEnumerable<string[]> ParseCSV(string csvFile, Action<TextFieldParser> builder)
    {
        using (TextFieldParser parser = new TextFieldParser(csvFile))
        {
            builder?.Invoke(parser);
            while (!parser.EndOfData)
            {
                yield return parser.ReadFields();
            }
        }
    }

    private DeckParserProgressReport TranslateProgress(CommandProgressReport arg)
        => arg.AsRatio<CommandProgressReport, DeckParserProgressReport>(10, 0.20f);
}
