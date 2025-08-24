using DocumentFormat.OpenXml.Bibliography;
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
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using Color = SixLabors.ImageSharp.Color;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck;

public class DeckTranslationDocumentExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DeckTranslationDocumentExporter>();
//    private static readonly DecoderOptions _decoderOptions = new DecoderOptions { };

    private readonly GlobalCookieJar _gcj;
    private readonly IFileOutCommandProcessor _fileProcessor;

    private readonly Func<Flurl.Url?, CookieSession?> _cookieSessionFunc;

    public string[] Alias => new[] { "trans-doc" };

    public DeckTranslationDocumentExporter(GlobalCookieJar globalCookieJar, IFileOutCommandProcessor fileOutCommandProcessor)
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

        var resultingDocFilePath = resultFolder.Combine($"translations_{fileNameFriendlyDeckName}.docx");
        using (WordDocument document = WordDocument.Create(filePath: resultingDocFilePath.FullPath, autoSave: true))
        {
            document.PageOrientation = PageOrientationValues.Portrait;
            document.PageSettings.PageSize = WordPageSize.A4;
            document.Margins.Top = 720;
            document.Margins.Bottom = 720;
            document.Margins.Left = 720;
            document.Margins.Right = 720;

            var table = document.AddTable(1, 2, WordTableStyle.TableGrid);
            //table.SetWidthPercentage(100);
            //table.ColumnWidthType = TableWidthUnitValues.Pct;

            foreach (var entry in deck.Ratios)
            {
                var card = entry.Key;

                await using var imageStream = await card.GetImageStreamAsync((card.Images.Count > 0) ? _cookieSessionFunc(card.Images.Last()) : null, cancellationToken);
                var rawImage = PreProcess(await Image.LoadAsync(imageStream, cancellationToken));

                using var rawImageStream = new System.IO.MemoryStream();
                rawImage.Save(rawImageStream, PngFormat.Instance);
                rawImageStream.TryGetBuffer(out ArraySegment<byte> buffer);

                var row = table.AddRow();
                
                row.Cells[0].Paragraphs[0].AddImageFromBase64(
                    Convert.ToBase64String(buffer.Array, 0, (int)rawImageStream.Length),
                    card.Serial + ".png",
                    width: 143d,
                    height: 200d, 
                    description: $"{card.Name.AsNonEmptyString()}"
                    );
                var descParagraph = row.Cells[1].Paragraphs[0];
                descParagraph.AddText(card.Name.AsNonEmptyString());
                descParagraph.AddBreak();
                if (card.Type != CardType.Climax)
                {
                    descParagraph.AddText($"Lv {card.Level}/ Co {card.Cost}");
                    if (card.Type == CardType.Character)
                    {
                        descParagraph.AddText($" || {card.Power} / {card.Soul}");
                        descParagraph.AddBreak();
                        descParagraph.AddText($"Traits: {card.Traits.Select(t => t.TraitString).ConcatAsString(" - ")}");
                    }
                    descParagraph.AddBreak();

                }
                descParagraph.AddText(card.Effect.ConcatAsString("\n"));
            }

            table.FirstRow.Remove();

            report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = "Drawing the table specifications..." } };
            progress.Report(report);

            table.ColumnWidthType = TableWidthUnitValues.Pct;

            var colWidths = table.ColumnWidth;
            colWidths[0] = 25 * 50;
            colWidths[1] = 75 * 50;
            table.ColumnWidth = colWidths;
            table.GridColumnWidth = colWidths;

            table.StyleDetails.SetBordersForAllSides(BorderValues.BasicWhiteDots, 1, Color.WhiteSmoke);

            report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = "Finalizing and saving document..." } };
            progress.Report(report);
        }

        Log.Information("Saved to {path}", resultingDocFilePath.FullPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Opening document: {resultingDocFilePath.FullPath}" } };
            progress.Report(report);
            System.Diagnostics.Process.Start("explorer", $"\"{resultingDocFilePath.FullPath}\"");
        } else
        {
            report = report with { ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Saved document: {resultingDocFilePath.FullPath}" } };
            progress.Report(report);
        }

            static void ApplyParagraphStyle(WordParagraph paragraph, CardColor color)
            {
                paragraph.ParagraphAlignment = JustificationValues.Both;
                paragraph.LineSpacingAfter = 0;
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
