using Fluent.IO;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.JSON;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    public class LocalDeckJSONParser : IDeckParser
    {
        private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONParser>();
        public string[] Alias => new[] { "local", "json" };
        public int Priority => 1;
        private readonly Func<CardDatabaseContext> _database;

        public LocalDeckJSONParser(IContainer container)
        {
            _database = () => container.GetInstance<CardDatabaseContext>();
        }


        public bool IsCompatible(string urlOrFile)
        {
            var filePath = Path.Get(urlOrFile);
            if (!filePath.Exists)
                return false;
            else if (filePath.Extension != ".json")
                return false;
            else
                return true;
        }

        public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            var filePath = Path.Get(sourceUrlOrFile);
            SimpleDeck deckJSON = null;
            deckJSON = JsonSerializer.Deserialize<SimpleDeck>(filePath.ReadBytes());
            WeissSchwarzDeck deck = new WeissSchwarzDeck();
            deck.Name = deckJSON.Name;
            deck.Remarks = deckJSON.Remarks;
            using (var db = _database())
            {
                await db.Database.MigrateAsync();
                foreach (var serial in deckJSON.Ratios.Keys)
                {
                    var card = await db.WeissSchwarzCards.FindAsync(serial);
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
            return deck;
        }
    }
}
