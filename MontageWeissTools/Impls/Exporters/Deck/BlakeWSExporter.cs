using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Inspectors.Deck;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;
public class BlakeWSExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>, IFilter<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>
{
    private static ILogger Log = Serilog.Log.ForContext<BlakeWSExporter>();
    private static string _exporterFormalName = "Weiss Schwarz by Blake";

    private Type[] _exclusionFilters = new[]
    {
        typeof(CachedImageInspector),
        typeof(SanityImageInspector),
        typeof(SanityTranslationsInspector),
        typeof(SanityTranslationsInspector),
        typeof(TTSCardCorrector)
    };

    private readonly WeissSchwarzBlakeUnityService _blakeWSSrvc;

    public string[] Alias => new string[] { "blake", "bws" };

    public BlakeWSExporter(IContainer ioc)
    {
        _blakeWSSrvc = ioc.GetInstance<WeissSchwarzBlakeUnityService>();
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        Log.Information("Starting...");
        var report = DeckExportProgressReport.Starting(deck.Name, _exporterFormalName);
        info.Progress.Report(report);

        Log.Information("Encoding information to deck...");
        var deckCodeString = deck.Ratios
            .SelectMany(p => Enumerable.Repeat(p.Key.Serial, p.Value))
            .Select(HandleExceptionalSerial)
            .ConcatAsString("|") + "|\0";
        var deckTime = DateTime.Now;

        Log.Information("Inserting data to [wstools import]...");
        Log.Information("Please ensure that the deck has been created manually via Blake. This CLI will not do it for you due to technical limitations.");

        _blakeWSSrvc.ExportDeckData(deckCodeString);
        _blakeWSSrvc.ExportDeckDate(deckTime);

        Log.Information("Done!");
        info.Progress.Report(report.Done());

        await Task.CompletedTask;
    }

    private string HandleExceptionalSerial(string serial)
    {
        return serial switch
        {
            var s when s.Contains("EN-W03") => s.Replace("EN-W03", "ENW03"),
            _ => serial
        };
    }

    public bool IsIncluded(IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard> item)
    {
        return item.GetType() switch
        {
            var t when _exclusionFilters.Contains(t) => false,
            _ => true
        };
    }
}
