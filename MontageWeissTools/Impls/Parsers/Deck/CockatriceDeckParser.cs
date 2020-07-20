using Fluent.IO;
using Lamar;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.Cockatrice;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    public class CockatriceDeckParser : IDeckParser
    {
        private readonly ILogger Log;
        private Func<CardDatabaseContext> _database;

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

        public bool IsCompatible(string urlOrFile)
        {
            var file = Fluent.IO.Path.Get(urlOrFile);
            return file.Exists && file.Extension == ".cod";
        }

        public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            Log.Information("Parsing: {source}", sourceUrlOrFile);
            var file = Fluent.IO.Path.Get(sourceUrlOrFile);
            var serializer = new XmlSerializer(typeof(CockatriceDeck));
            using (var stream = file.GetStream())
            {
                var cockatriceDeck = serializer.Deserialize(stream) as CockatriceDeck;
                var result = new WeissSchwarzDeck();
                result.Name = cockatriceDeck.DeckName;
                result.Remarks = cockatriceDeck.Comments;
                result.Ratios = cockatriceDeck.Ratios.Ratios.Select(Translate).ToDictionary(p => p.card, p => p.amount);
                return result;
            }
        }

        private (WeissSchwarzCard card, int amount) Translate(CockatriceSerialAmountPair pair)
        {
            using (var db = _database())
            {
                return (db.WeissSchwarzCards.Find(pair.Serial), pair.Amount);
            }
        }
    }
}
