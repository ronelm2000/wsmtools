using Fluent.IO;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Text.Json;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

/// <summary>
/// A Deck Exporter whose output is purely a JSON file that coincides with the format of WeissSchwarzDeck, exactly.
/// </summary>
public class LocalDeckJSONExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONExporter>();
    private JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
    {
        WriteIndented = true
    };
    private readonly Func<IFileOutCommandProcessor> _focProcessor;

    public string[] Alias => new[]{ "local", "json" };

    public LocalDeckJSONExporter(IContainer ioc)
    {
        _focProcessor = () => ioc.GetInstance<IFileOutCommandProcessor>();
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        Log.Information("Exporting as Deck JSON.");
        var report = DeckExportProgressReport.Starting(deck.Name, ".dek Exporter");
        var progress = info.Progress;
        progress.Report(report);

        var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.json");
        var simplifiedDeck = new
        {
            Name = deck.Name,
            Remarks = deck.Remarks,
            Ratios = deck.AsSimpleDictionary()
        };

        jsonFilename.Open(
            async s => await JsonSerializer.SerializeAsync(s, simplifiedDeck, options: _defaultOptions, cancellationToken),
            System.IO.FileMode.Create,
            System.IO.FileAccess.Write,
            System.IO.FileShare.ReadWrite
        );

        Log.Information($"Done: {jsonFilename.FullPath}");
        report = report.Done(jsonFilename.FullPath);
        progress.Report(report);

        if (!String.IsNullOrWhiteSpace(info.OutCommand))
            await _focProcessor().Process(info.OutCommand, jsonFilename.FullPath, cancellationToken);
    }
}
