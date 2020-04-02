using Flurl;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    public class EncoreDecksParser : IDeckParser
    {
        private ILogger Log { get; }

        // Dependencies
        private readonly Func<CardDatabaseContext> _database;
        private readonly Func<string,Task> _parse;

        public string[] Alias => new[] { "encoredecks", "ed" };
        public int Priority => 1;

        public EncoreDecksParser(IContainer container)
        {
            Log = Serilog.Log.ForContext<EncoreDecksParser>();
            _database = () => container.GetInstance<CardDatabaseContext>();
            _parse = async (url) =>
            {
                var parser = container.GetInstance<ParseVerb>();
                parser.URI = url;
                await parser.Run(container);
            };
        }
        

        public bool IsCompatible(string urlOrFile)
        {
            if (urlOrFile == null) return false;

            return (File.Exists(urlOrFile) && urlOrFile.EndsWith(".csv")) ||
                urlOrFile.StartsWith("https://www.encoredecks.com/deck/");
        }

        public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            if (File.Exists(sourceUrlOrFile))
                    return await ParseFromCSV(sourceUrlOrFile);
            else
                return await Parse(new Uri(sourceUrlOrFile));
        }

        private async Task<WeissSchwarzDeck> ParseFromCSV(string sourceCSV)
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

        private async Task<WeissSchwarzDeck> Parse(Uri uri)
        {
            var encoreDecksDeckAPIURL = "https://www.encoredecks.com/api/deck";

            var localPath = uri.LocalPath;
            var deckID = localPath.Substring(localPath.LastIndexOf('/') + 1);
            Log.Information("Deck ID: {deckID}", deckID);
            dynamic deckJSON = await encoreDecksDeckAPIURL
                .AppendPathSegment(deckID)
                .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                .WithHeader("Accept", "text/plain")
                .GetJsonAsync<dynamic>();

            WeissSchwarzDeck res = new WeissSchwarzDeck();
            res.Name = deckJSON.name;

            using (var db = _database())
            {
                await db.Database.MigrateAsync();

                foreach (dynamic card in deckJSON.cards)
                {
                    string serial = WeissSchwarzCard.GetSerial(card.set.ToString(), card.side.ToString(), card.lang.ToString(), card.release.ToString(), card.sid.ToString());
                    WeissSchwarzCard wscard = await db.WeissSchwarzCards.FindAsync(serial);
                    if (wscard == null)
                    {
                        string setID = card.series;
                        await _parse($"https://www.encoredecks.com/api/series/{setID}/cards");
                        wscard = await db.WeissSchwarzCards.FindAsync(serial);
                    }

                    if (res.Ratios.TryGetValue(wscard, out int quantity))
                        res.Ratios[wscard]++;
                    else
                        res.Ratios[wscard] = 1;

                    //Log.Information("Parsed: {@wscard}", wscard);

                }
            }
            var simpleRatios = res.AsSimpleDictionary();
            Log.Information("Deck Parsed: {@simpleRatios}", simpleRatios);
            Log.Information("Cards in Deck: {@count}", simpleRatios.Values.Sum());
            return res;
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

    }
}
