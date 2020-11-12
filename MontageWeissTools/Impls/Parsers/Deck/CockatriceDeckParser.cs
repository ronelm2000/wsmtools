using Fluent.IO;
using Lamar;
using Microsoft.EntityFrameworkCore.Update;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.Exceptions;
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
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    public class CockatriceDeckParser : IDeckParser
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
                    return await Task.FromResult(result);
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
}
