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
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck
{
    /// <summary>
    /// A Deck Exporter whose output is purely an image file more suited for sharing over SNS / social media.
    /// </summary>
    public class LocalDeckImageExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
    {
        public string[] Alias => new[] { "local_image", "image" };
        private ILogger Log = Serilog.Log.ForContext<LocalDeckImageExporter>();
        private (IImageEncoder, IImageFormat) _pngEncoder = (new PngEncoder(), PngFormat.Instance);
        private (IImageEncoder, IImageFormat) _jpegEncoder = (new JpegEncoder(), JpegFormat.Instance);
        private readonly Func<Flurl.Url, CookieSession> _cookieSession;
        private readonly Func<string, string, Task> _processOutCommand;

        public LocalDeckImageExporter(IContainer ioc)
        {
            _cookieSession = (url) => ioc.GetInstance<GlobalCookieJar>()[url.Root];
            _processOutCommand = ioc.GetInstance<IFileOutCommandProcessor>().Process;
        }

        public async Task Export(WeissSchwarzDeck deck, IExportInfo info)
        {
            Log.Information("Exporting as Deck Image.");
            //var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.jpg");
            var count = deck.Ratios.Keys.Count;
            int rows = (int)Math.Ceiling(deck.Count / 10d);
            var serialList = AsOrdered(deck.Ratios.Keys)
                .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c))
                .ToList();
            var resultFolder = Path.CreateDirectory(info.Destination);
            var fileNameFriendlyDeckName = deck.Name.AsFileNameFriendly();
            var imageDictionary = await AsOrdered(deck.Ratios.Keys)
                .ToAsyncEnumerable()
                .Select((p, i) =>
                {
                    Log.Information("Loading Images: ({i}/{count}) [{serial}]", i + 1, count, p.Serial);
                    return p;
                })
                .SelectAwait(async (wsc) => (card: wsc, stream: await wsc.GetImageStreamAsync(_cookieSession(wsc.Images.Last()))))
                .ToDictionaryAsync(p => p.card, p => PreProcess(Image.Load(p.stream)));

            var (encoder, format) = info.Flags.Any(s => s.ToLower() == "png") == true ? _pngEncoder : _jpegEncoder;
            var newImageFilename = $"deck_{fileNameFriendlyDeckName.ToLower()}.{format.FileExtensions.First()}";
            var deckImagePath = resultFolder.Combine(newImageFilename);
            GenerateDeckImage(info, rows, serialList, imageDictionary, encoder, deckImagePath);

            if (info.OutCommand != "")
                await _processOutCommand(info.OutCommand, deckImagePath.FullPath);
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

        private void GenerateDeckImage(IExportInfo info, int rows, List<WeissSchwarzCard> serialList, Dictionary<WeissSchwarzCard, Image> imageDictionary, IImageEncoder encoder, Path deckImagePath)
        {
            using (var _ = imageDictionary.GetDisposer())
            {
                var selection = imageDictionary.Select(p => (p.Value.Width, p.Value.Height));
                (int Width, int Height) bounds = (0, 0);
                if (info.Flags.Contains("upscaling"))
                {
                    bounds = selection.Aggregate((a, b) => (Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height)));
                    Log.Information("Adjusting image sizing to the maximum bounds: {@minimumBounds}", bounds);
                }
                else
                {
                    bounds = selection.Aggregate((a, b) => (Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height)));
                    Log.Information("Adjusting image sizing to the minimum bounds: {@minimumBounds}", bounds);
                }
                foreach (var image in imageDictionary.Values)
                    image.Mutate(x => x.Resize(bounds.Width, bounds.Height));

                var grid = (Width: bounds.Width * 10, Height: bounds.Height * rows);
                Log.Information("Creating Full Grid of {x}x{y}...", grid.Width, grid.Height);

                using (var fullGrid = new Image<Rgba32>(bounds.Width * 10, bounds.Height * rows))
                {
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
                    deckImagePath.Open(s => fullGrid.Save(s, encoder));

                    if (Program.IsOutputRedirected) // Enable Non-Interactive Path stdin Passthrough of the deck png
                        using (var stdout = Console.OpenStandardOutput())
                            fullGrid.Save(stdout, encoder);

                    Log.Information($"Done! Result PNG: {deckImagePath.FullPath}");
                }
            }
        }

    }
}
