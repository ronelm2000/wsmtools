using Fluent.IO;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.Cockatrice;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Utilities;
using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

public class CockatriceDeckExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>
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
    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        Log.Information("Serializing: {name}", deck.Name);
        var progress = info.Progress;
        var report = DeckExportProgressReport.Starting(deck.Name, "Cockatrice");
        progress.Report(report);

        using (var db = _database())
        {
            Log.Information("Replacing all foils with non-foils...");
            report = report with
            {
                Percentage = 1,
                ReportMessage = new MultiLanguageString { EN = "Replacing foils with non-foils..." }
            };
            progress.Report(report);

            foreach (var card in deck.Ratios.Keys)
                if (card.IsFoil && ((await db.FindNonFoil(card, cancellationToken)) is WeissSchwarzCard nonFoilCard))
                    deck.ReplaceCard(card, nonFoilCard);
        }

        report = report with
        {
            Percentage = 30,
            ReportMessage = new MultiLanguageString { EN = "Creating Deck for COD format." }
        };
        progress.Report(report);

        Log.Information("Creating deck.cod...");
        var cckDeck = new CockatriceDeck();
        cckDeck.DeckName = deck.Name;
        cckDeck.Comments = deck.Remarks;
        cckDeck.Ratios = new CockatriceDeckRatio();
        cckDeck.Ratios.Ratios = deck.Ratios.Select(Translate).ToList();

        report = report with
        {
            Percentage = 30,
            ReportMessage = new MultiLanguageString { EN = "Saving COD file..." }
        };
        progress.Report(report);

        var deckFilename = deck.Name?.AsFileNameFriendly();
        if (String.IsNullOrEmpty(deckFilename)) deckFilename = "deck";
        var resultDeck = Path.CreateDirectory(info.Destination).Combine($"{deckFilename}.cod");
        await resultDeck.WriteAsync(s => _serializer.Serialize(s, cckDeck), cancellationToken);
        Log.Information($"Saved: {resultDeck.FullPath}");

        report = report.Done(resultDeck.FullPath);
        progress.Report(report);
    }

    private Type[] _exclusionFilters = new[]
    {
        typeof(CachedImageInspector),
        typeof(SanityImageInspector), 
        typeof(SanityTranslationsInspector)
    };
    public bool IsIncluded(IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard> item)
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
