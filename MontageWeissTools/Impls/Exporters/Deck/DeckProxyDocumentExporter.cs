using DocumentFormat.OpenXml.Wordprocessing;
using Fluent.IO;
using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using OfficeIMO.Word;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

public class DeckProxyDocumentExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DeckProxyDocumentExporter>();
//    private static readonly DecoderOptions _decoderOptions = new DecoderOptions { };

    private readonly GlobalCookieJar _gcj;
    private readonly IFileOutCommandProcessor _fileProcessor;

    private readonly Func<Flurl.Url, CookieSession> _cookieSessionFunc;

    public string[] Alias => new[] { "doc", "proxy-doc" };

    public DeckProxyDocumentExporter(GlobalCookieJar globalCookieJar, IFileOutCommandProcessor fileOutCommandProcessor)
    {
        _gcj = globalCookieJar ?? throw new ArgumentNullException(nameof(globalCookieJar));
        _fileProcessor = fileOutCommandProcessor ?? throw new ArgumentNullException(nameof(fileOutCommandProcessor));

        _cookieSessionFunc = (url) => _gcj[url.Root];
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        Log.Information("Exporting \"{deckName}\" to proxy document.", deck.Name);

        var progress = info.Progress;
        var report = DeckExportProgressReport.Starting(deck.Name, "Deck Proxy Document Exporter");
        progress.Report(report);


        /*
        var document = new Openize.Words.Document();
        var body = new Openize.Words.Body(document);
        var paragraph = new Openize.Words.IElements.Paragraph();

        var table = new Openize.Words.IElements.Table(3, 3);
        */

        //      body.AppendChild(paragraph);

        var count = deck.Ratios.Keys.Count;
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
            (card: wsc,
                stream: await wsc.GetImageStreamAsync(_cookieSessionFunc(wsc.Images.Last()), ct))
            )
            .ToDictionaryAwaitWithCancellationAsync(
                async (p, ct) => await ValueTask.FromResult(p.card),
                async (p, ct) => PreProcess(await Image.LoadAsync(p.stream, ct)),
                cancellationToken
            );

        var resultingDocFilePath = resultFolder.Add($"proxy_{fileNameFriendlyDeckName}.docx");
        using (WordDocument document = WordDocument.Create(filePath: resultingDocFilePath.FullPath, autoSave: true))
        {
            document.PageOrientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;
            document.Margins.Top = 720;
            document.Margins.Bottom = 720;
            document.Margins.Left = 720;
            document.Margins.Right = 720;

            var paragraph = document.AddParagraph();

            foreach (var entry in deck.Ratios)
            {
                var card = entry.Key;
                var quantity = entry.Value;
                Log.Information("Adding {quantity} copies of {cardSerial} to the document.", quantity, card.Serial);
                report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Adding {quantity} copies of {card.Serial} to the document." } };
                progress.Report(report);

                await using var imageStream = await card.GetImageStreamAsync(_cookieSessionFunc(card.Images.Last()), cancellationToken);
                var rawImage = PreProcess(await Image.LoadAsync(imageStream, cancellationToken));

                var tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
                await rawImage.SaveAsBmpAsync(tempImagePath, cancellationToken);

                for (int i = 0; i < quantity; i++)
                {
                    paragraph.AddImage(tempImagePath, width: 240d, height: 335d);
                }
            }
        }

        Log.Information("Saved to {path}", resultingDocFilePath.FullPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            System.Diagnostics.Process.Start("explorer", $"\"{resultingDocFilePath.FullPath}\"");

        // throw new NotImplementedException();
    }

    private IEnumerable<WeissSchwarzCard> AsOrdered(IEnumerable<WeissSchwarzCard> cards)
        => cards
            .OrderBy(card => card.Type.GetSortKey())
            .ThenByDescending(card => card.Level)
            .ThenByDescending(card => card.Cost)
            .ThenBy(card => card.Color.GetSortKey())
            .ThenBy(card => card.Serial);

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
}
