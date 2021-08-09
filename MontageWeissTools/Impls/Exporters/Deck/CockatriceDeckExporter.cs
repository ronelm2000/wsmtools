using Fluent.IO;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.Cockatrice;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Utilities;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck
{
    public class CockatriceDeckExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IExportedDeckInspector>
    {
        private readonly ILogger Log;
        private Func<CardDatabaseContext> _database;
        private XmlSerializer _serializer = new XmlSerializer(typeof(CockatriceDeck));

        public string[] Alias => new[] { "cockatrice", "cckt3s" };

        public CockatriceDeckExporter(IContainer container)
        {
            Log = Serilog.Log.ForContext<CockatriceDeckExporter>();
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
        public async Task Export(WeissSchwarzDeck deck, IExportInfo info)
        {
            Log.Information("Serializing: {name}", deck.Name);

            using (var db = _database())
            {
                Log.Information("Replacing all foils with non-foils...");
                foreach (var card in deck.Ratios.Keys)
                    if (card.IsFoil) deck.ReplaceCard(card, await db.FindNonFoil(card));
            }

            Log.Information("Creating deck.cod...");
            var cckDeck = new CockatriceDeck();
            cckDeck.DeckName = deck.Name;
            cckDeck.Comments = deck.Remarks;
            cckDeck.Ratios = new CockatriceDeckRatio();
            cckDeck.Ratios.Ratios = deck.Ratios.Select(Translate).ToList();

            var deckFilename = deck.Name?.AsFileNameFriendly();
            if (String.IsNullOrEmpty(deckFilename)) deckFilename = "deck";
            var resultDeck = Path.CreateDirectory(info.Destination).Combine($"{deckFilename}.cod");
            resultDeck.Open(s => _serializer.Serialize(s, cckDeck),
                                    System.IO.FileMode.Create,
                                    System.IO.FileAccess.Write,
                                    System.IO.FileShare.ReadWrite
                                    );
            Log.Information($"Saved: {resultDeck.FullPath}");
        }

        private Type[] _exclusionFilters = new[]
        {
            typeof(CachedImageInspector),
            typeof(SanityImageInspector), 
            typeof(SanityTranslationsInspector)
        };
        public bool IsIncluded(IExportedDeckInspector item)
        {
            return item.GetType() switch
            {
                var t when _exclusionFilters.Contains(t) => false,
                _ => true
            };
        }

        private CockatriceSerialAmountPair Translate(KeyValuePair<WeissSchwarzCard, int> cardAmountPair)
        {
            return new CockatriceSerialAmountPair()
            {
                Serial = WeissSchwarzCard.AsJapaneseSerial(cardAmountPair.Key.Serial),
                Amount = cardAmountPair.Value
            };
        }


    }
}
