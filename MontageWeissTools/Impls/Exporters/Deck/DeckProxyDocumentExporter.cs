using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Wordprocessing;
using Fluent.IO;
using Flurl.Http;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Utilities;
using OfficeIMO.Word;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using Color = SixLabors.ImageSharp.Color;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

public class DeckProxyDocumentExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DeckProxyDocumentExporter>();
//    private static readonly DecoderOptions _decoderOptions = new DecoderOptions { };

    private readonly GlobalCookieJar _gcj;
    private readonly IFileOutCommandProcessor _fileProcessor;

    private readonly Func<Flurl.Url?, CookieSession?> _cookieSessionFunc;

    public string[] Alias => new[] { "doc", "proxy-doc" };

    public DeckProxyDocumentExporter(GlobalCookieJar globalCookieJar, IFileOutCommandProcessor fileOutCommandProcessor)
    {
        _gcj = globalCookieJar ?? throw new ArgumentNullException(nameof(globalCookieJar));
        _fileProcessor = fileOutCommandProcessor ?? throw new ArgumentNullException(nameof(fileOutCommandProcessor));
        _cookieSessionFunc = (url) => url is null ? null : _gcj[url!.Root];
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
            (   card: wsc,
                stream: await wsc.GetImageStreamAsync( (wsc.Images.Count > 0) ? _cookieSessionFunc(wsc.Images.Last()) : null, ct))
            )
            .ToDictionaryAwaitWithCancellationAsync(
                async (p, ct) => await ValueTask.FromResult(p.card),
                async (p, ct) => PreProcess(await Image.LoadAsync(p.stream, ct)),
                cancellationToken
            );

        var resultingDocFilePath = resultFolder.Combine($"proxy_{fileNameFriendlyDeckName}.docx");
        using (WordDocument document = WordDocument.Create(filePath: resultingDocFilePath.FullPath, autoSave: true))
        {
            document.PageOrientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;
            document.Margins.Top = 720;
            document.Margins.Bottom = 720;
            document.Margins.Left = 720;
            document.Margins.Right = 720;

            var paragraph = document.AddParagraph();
            var baseCardNo = 0;

            foreach (var entry in deck.Ratios)
            {
                var card = entry.Key;
                var quantity = entry.Value;
                Log.Information("Adding {quantity} copies of {cardSerial} to the document.", quantity, card.Serial);
                report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Adding {quantity} copies of {card.Serial} to the document." } };
                progress.Report(report);

                await using var imageStream = await card.GetImageStreamAsync( (card.Images.Count > 0) ? _cookieSessionFunc(card.Images.Last()) : null, cancellationToken);
                var rawImage = PreProcess(await Image.LoadAsync(imageStream, cancellationToken));

                var tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
                await rawImage.SaveAsBmpAsync(tempImagePath, cancellationToken);

                for (int i = 0; i < quantity; i++)
                {
                    paragraph.AddImage(tempImagePath, width: 241d, height: 336d, description: $"{card.Name.AsNonEmptyString()}");
                    var image = paragraph.Image;
                    if (card.Type != CardType.Climax && card.Language == CardLanguage.Japanese)
                    {
                        var textBox = paragraph.AddTextBox(card.Effect.FirstOrDefault()?.Trim() ?? "", WrapTextImage.InFrontOfText);
                        var textBoxParagraph = textBox.Paragraphs[0];

                        var currentParagraph = textBoxParagraph;
                        for (var j = 1; j < card.Effect.Length; j++)
                        {
                            if (string.IsNullOrEmpty(card.Effect[j]))
                                continue;

                            currentParagraph = currentParagraph.AddText(card.Effect[j]);
                            ApplyParagraphStyle(currentParagraph, card.Color);

                            currentParagraph = currentParagraph.AddBreak();
                            ApplyParagraphStyle(currentParagraph, card.Color);
                        }

                        ApplyParagraphStyle(textBoxParagraph, card.Color);

                        textBox.AutoFitToTextSize = false;
                        textBox.HorizontalPositionRelativeFrom = HorizontalRelativePositionValues.Character;
                        textBox.VerticalPositionRelativeFrom = VerticalRelativePositionValues.Line;
                        textBox.HorizontalPositionOffset = (int)(0.45d * 360000.0d) - 135000;
                        textBox.VerticalPositionOffset = (3 * 360000) - 100000;
                        textBox.WidthCentimeters = 6.25d;
                        textBox.HeightCentimeters = 5.75d;
                        textBox.TextBodyProperties.Anchor = DocumentFormat.OpenXml.Drawing.TextAnchoringTypeValues.Bottom;
                    }
                    paragraph = paragraph.AddText(" ");
                }

                baseCardNo += quantity;
            }
        }

        Log.Information("Saved to {path}", resultingDocFilePath.FullPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            System.Diagnostics.Process.Start("explorer", $"\"{resultingDocFilePath.FullPath}\"");

        static void ApplyParagraphStyle(WordParagraph paragraph, CardColor color)
        {
            paragraph.ParagraphAlignment = JustificationValues.Left;
            paragraph.Color = TranslateToColor(color);
            paragraph.Highlight = TranslateToHighlight(color);
            paragraph.FontSize = 5;
            paragraph.Spacing = -1;
            paragraph.FontFamily = "Calibri";
        }

        static Color TranslateToColor(CardColor color)
        {
            return color switch
            {
                CardColor.Yellow => Color.Black,
                CardColor.Green => Color.Black,
                CardColor.Red => Color.White,
                CardColor.Blue => Color.White,
                CardColor.Purple => Color.White,
                _ => Color.Black
            };
        }
        static HighlightColorValues TranslateToHighlight(CardColor color)
        {
            return color switch
            {
                CardColor.Yellow => HighlightColorValues.Yellow,
                CardColor.Green => HighlightColorValues.Green,
                CardColor.Red => HighlightColorValues.Red,
                CardColor.Blue => HighlightColorValues.DarkBlue,
                CardColor.Purple => HighlightColorValues.DarkCyan,
                _ => HighlightColorValues.White
            };
        }
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
