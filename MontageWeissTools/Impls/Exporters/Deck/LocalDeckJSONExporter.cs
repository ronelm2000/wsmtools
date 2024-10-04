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

    public string[] Alias => new[] { "local", "json" };

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

        var destination = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.ws-dek");
        await Export(deck, report, progress, info.OutCommand, destination, cancellationToken);
    }

    public async Task Export(WeissSchwarzDeck deck, IProgress<DeckExportProgressReport> progress, Path jsonPath, CancellationToken cancellationToken = default)
    {
        Log.Information("Exporting as Deck JSON.");
        var report = DeckExportProgressReport.Starting(deck.Name, ".dek Exporter");
        progress.Report(report);

        await Export(deck, report, progress, null, jsonPath, cancellationToken);
    }

    private async Task Export(
        WeissSchwarzDeck deck, 
        DeckExportProgressReport report, 
        IProgress<DeckExportProgressReport> progress,
        string? outCommand, 
        Path jsonPath,
        CancellationToken cancellationToken
        )
    {
        var simplifiedDeck = new
        {
            Name = deck.Name,
            Remarks = deck.Remarks,
            Ratios = deck.AsSimpleDictionary()
        };

        jsonPath.Open(
            async s => await JsonSerializer.SerializeAsync(s, simplifiedDeck, options: _defaultOptions, cancellationToken),
            System.IO.FileMode.Create,
            System.IO.FileAccess.Write,
            System.IO.FileShare.ReadWrite
        );

        Log.Information($"Done: {jsonPath.FullPath}");
        report = report.Done(jsonPath.FullPath);
        progress.Report(report);

        if (!String.IsNullOrWhiteSpace(outCommand))
            await _focProcessor().Process(outCommand, jsonPath.FullPath, cancellationToken);
    }
}
