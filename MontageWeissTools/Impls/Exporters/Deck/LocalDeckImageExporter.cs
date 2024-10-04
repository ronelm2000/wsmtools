using Fluent.IO;
using Flurl.Http;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SQLitePCL;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

/// <summary>
/// A Deck Exporter whose output is purely an image file more suited for sharing over SNS / social media.
/// </summary>
public class LocalDeckImageExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private static readonly DecoderOptions _decoderOptions = new DecoderOptions { };

    public string[] Alias => new[] { "local_image", "image" };
    private ILogger Log = Serilog.Log.ForContext<LocalDeckImageExporter>();
    private (IImageEncoder, IImageFormat) _pngEncoder = (new PngEncoder(), PngFormat.Instance);
    private (IImageEncoder, IImageFormat) _jpegEncoder = (new JpegEncoder(), JpegFormat.Instance);
    private readonly Func<Flurl.Url, CookieSession> _cookieSession;
    private readonly Func<string, string, CancellationToken, Task> _processOutCommand;

    private readonly Regex limitFlagRegex = new Regex(@"limit-width\((\d+)\)");

    public LocalDeckImageExporter(IContainer ioc)
    {
        _cookieSession = (url) => ioc.GetInstance<GlobalCookieJar>()[url.Root];
        _processOutCommand = ioc.GetInstance<IFileOutCommandProcessor>().Process;
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        Log.Information("Exporting as Deck Image.");
        var progress = info.Progress;
        var report = DeckExportProgressReport.Starting(deck.Name, "Deck Image Exporter");
        progress.Report(report);

        //var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.jpg");
        var count = deck.Ratios.Keys.Count;
        int rows = (int)Math.Ceiling(deck.Count / 10d);
        var serialList = AsOrdered(deck.Ratios.Keys)
            .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c))
            .ToList();
        var resultFolder = Path.CreateDirectory(info.Destination);
        var fileNameFriendlyDeckName = deck.Name.AsFriendlyToTabletopSimulator();
        var imageDictionary = await AsOrdered(deck.Ratios.Keys)
            .ToAsyncEnumerable()
            .Select((p, i) =>
            {
                Log.Information("Loading Images: ({i}/{count}) [{serial}]", i + 1, count, p.Serial);
                report = report.LoadingImages(p.Serial, i + 1, count);
                progress.Report(report);
                return p;
            })
            .SelectAwaitWithCancellation(async (wsc, ct) => 
            (   card: wsc, 
                stream: await wsc.GetImageStreamAsync(_cookieSession(wsc.Images.Last()), ct))
            )
            .ToDictionaryAwaitWithCancellationAsync(
                async (p, ct) => await ValueTask.FromResult(p.card),
                async (p, ct) => PreProcess(await Image.LoadAsync(_decoderOptions, p.stream, ct)),
                cancellationToken
                );
            //.ToDictionaryAsync(p => p.card, p => PreProcess(Image.LoadAsync(p.stream)));

        var (encoder, format) = info.Flags.Any(s => s.ToLower() == "png") == true ? _pngEncoder : _jpegEncoder;
        var newImageFilename = $"deck_{fileNameFriendlyDeckName.ToLower()}.{format.FileExtensions.First()}";
        var deckImagePath = resultFolder.Combine(newImageFilename);
        await GenerateDeckImage(info, new(rows, serialList, imageDictionary, encoder, deckImagePath, progress, report, cancellationToken));

        if (info.OutCommand != "")
            await _processOutCommand(info.OutCommand, deckImagePath.FullPath, cancellationToken);
    }

    private IEnumerable<WeissSchwarzCard> AsOrdered(IEnumerable<WeissSchwarzCard> cards)
        => cards
            .OrderBy(card => card.Type.GetSortKey())
            .ThenByDescending(card => card.Level)
            .ThenByDescending(card => card.Cost)
            .ThenBy(card => card.Color.GetSortKey())
            .ThenBy(card => card.Serial)
        ;

    private Image PreProcess(Image image)
    {
        if (image.Height < image.Width)
        {
            Log.Debug("Image is probably incorrectly oriented, rotating it 90 degs. clockwise to compensate.");
            image.Mutate(ipc => ipc.Rotate(90));
        }

        var aspectRatio = (image.Width * 1.0d) / image.Height;
        var flooredAspectRatio = Math.Floor(aspectRatio * 100);
        if (flooredAspectRatio < 70)
        {
            var magicWeissRatio = 0.71428571428f;
            image.Mutate(ctx =>
            {
                ctx.Resize(image.Width, (int)Math.Floor(image.Width * magicWeissRatio));
            });
        }
        return image;
    }

    internal async Task GenerateDeckImage(IExportInfo info, DeckImageExporterArgs args)
    {
        var (rows, serialList, imageDictionary, encoder, deckImagePath, progress, _, tc) = args;

        var report = args.Report.GeneratingDeckImage();
        progress.Report(report);

        using var _ = imageDictionary.GetDisposer();

        var selection = imageDictionary.Select(p => (p.Value.Width, p.Value.Height));
        (int Width, int Height) bounds = (0, 0);
        if (info.Flags.Contains("upscaling"))
        {
            bounds = selection.Aggregate((a, b) => (Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height)));
            Log.Information("Adjusting image sizing to the maximum bounds: {@minimumBounds}", bounds);
            report = report.SizingImages("Upscaling", bounds);
            progress.Report(report);
        }
        else if (info.Flags.Any(limitFlagRegex.IsMatch))
        {
            var limitString = info.Flags.First(limitFlagRegex.IsMatch);
            var limitValueString = limitFlagRegex.Match(limitString).Groups[1].Value;
            var limitWidth = int.Parse(limitValueString);
            var maxBounds = selection.Aggregate((a, b) => (Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height)));
            bounds = (limitWidth, (int)((float)maxBounds.Height / (float)maxBounds.Width * limitWidth));
            Log.Information("Adjusting image sizing to the limited bounds: {@minimumBounds}", bounds);
        } 
        else
        {
            bounds = selection.Aggregate((a, b) => (Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height)));
            Log.Information("Adjusting image sizing to the minimum bounds: {@minimumBounds}", bounds);
            report = report.SizingImages("Downscaling", bounds);
            progress.Report(report);
        }
        foreach (var image in imageDictionary.Values)
            image.Mutate(x => x.Resize(bounds.Width, bounds.Height));

        var grid = (Width: bounds.Width * 10, Height: bounds.Height * rows);

        Log.Information("Creating Full Grid of {x}x{y}...", grid.Width, grid.Height);
        report = report.SizingImages("Downscaling", bounds);
        progress.Report(report);

        using var fullGrid = new Image<Rgba32>(bounds.Width * 10, bounds.Height * rows);

        for (int i = 0; i < serialList.Count; i++)
        {
            var x = i % 10;
            var y = i / 10;
            var point = new Point(x * bounds.Width, y * bounds.Height);

            fullGrid.Mutate(ctx =>
            {
                ctx.DrawImage(imageDictionary[serialList[i]], point, 1);
            });
        }

        Log.Information("Finished drawing all cards in logical order; saving image...");
        await deckImagePath.WriteAsync(s => fullGrid.SaveAsync(s, encoder, tc), tc);

        if (Program.IsOutputRedirected) // Enable Non-Interactive Path stdin Passthrough of the deck png
            using (var stdout = Console.OpenStandardOutput())
                await fullGrid.SaveAsync(stdout, encoder, tc);

        Log.Information($"Done! Result PNG: {deckImagePath.FullPath}");
        report = report.Done(deckImagePath.FullPath);
        progress.Report(report);
    }

    internal record DeckImageExporterArgs (
        int Rows, 
        List<WeissSchwarzCard> SerialList, 
        Dictionary<WeissSchwarzCard, Image> ImageDictionary, 
        IImageEncoder Encoder, 
        Path DeckImagePath,
        IProgress<DeckExportProgressReport> Progress,
        DeckExportProgressReport Report,
        CancellationToken CancellationToken
        );

}
