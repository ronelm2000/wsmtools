using Fluent.IO;
using Flurl.Http;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.TTS;
using Montage.Weiss.Tools.Resources;
using Montage.Weiss.Tools.Resources.TTS;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.IO;
//using System.IO;
using System.Linq;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters
{
    public class TTSDeckExporter : IDeckExporter
    {
        private ILogger Log = Serilog.Log.ForContext<TTSDeckExporter>();

        private (IImageEncoder, IImageFormat) _pngEncoder = (new PngEncoder(), PngFormat.Instance);
        private (IImageEncoder, IImageFormat) _jpegEncoder = (new JpegEncoder(), JpegFormat.Instance);

        public string[] Alias => new [] { "tts", "tabletopsim" };
        
        public async Task Export(WeissSchwarzDeck deck, IExportInfo info)
        {
            var count = deck.Ratios.Keys.Count;
            int rows = (int) Math.Ceiling(deck.Count / 10d);

            var serialList = deck.Ratios.Keys
                .OrderBy(c => c.Serial)
                .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c))
                .ToList();

            var resultFolder = Path.CreateDirectory(info.Destination);

            var fileNameFriendlyDeckName = deck.Name.AsFileNameFriendly();

            var imageDictionary = await deck.Ratios.Keys
                .OrderBy(c => c.Serial)
                .ToAsyncEnumerable()
                .Select((p, i) =>
                {
                    Log.Information("Loading Images: ({i}/{count}) [{serial}]", i, count, p.Serial);
                    return p;
                })
                .SelectAwait(async (wsc) => (card: wsc, stream: await wsc.GetImageStreamAsync()))
                .ToDictionaryAsync(p => p.card, p => Image.Load(p.stream));

            var (encoder, format) = info.Flags.Any(s => s.ToLower() == "png") == true ? _pngEncoder : _jpegEncoder;
            var newImageFilename = $"deck_{fileNameFriendlyDeckName.ToLower()}.{format.FileExtensions.First()}";
            var deckImagePath = resultFolder.Combine(newImageFilename);

            using (var _ = imageDictionary.GetDisposer())
            {
                var minimumBounds = imageDictionary .Select(p => (p.Value.Width, p.Value.Height))
                                                    .Aggregate((a, b) => (Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height)));

                Log.Information("Adjusting image sizing to the minimum bounds: {@minimumBounds}", minimumBounds);

                foreach (var image in imageDictionary.Values)
                    image.Mutate(x => x.Resize(minimumBounds.Width, minimumBounds.Height));
                
                var grid = (Width: minimumBounds.Width * 10, Height: minimumBounds.Height * rows);
                Log.Information("Creating Full Grid of {x}x{y}...", grid.Width, grid.Height);

                using (var fullGrid = new Image<Rgba32>(minimumBounds.Width * 10, minimumBounds.Height * rows))
                {
                    for (int i = 0; i < serialList.Count; i++)
                    {
                        var x = i % 10;
                        var y = i / 10;
                        var point = new Point(x * minimumBounds.Width, y * minimumBounds.Height);

                        fullGrid.Mutate(ctx =>
                        {
                            ctx.DrawImage(imageDictionary[serialList[i]], point, 1);
                        });
                    }
                   
                    Log.Information("Finished drawing all cards in serial order; saving image...");
                    deckImagePath.Open(s => fullGrid.Save(s, encoder));

                    if (Program.IsOutputRedirected) // Enable Non-Interactive Path stdin Passthrough of the deck png
                        using (var stdout = Console.OpenStandardOutput())
                            fullGrid.Save(stdout, encoder);

                    Log.Information($"Done! Result PNG: {deckImagePath.FullPath}");
                }
            }

            Log.Information("Generating the Custom Object for TTS...");

            var serialDictionary = deck.Ratios.Keys
                .ToDictionary(  card => card.Serial,
                                card => new
                                {
                                    Name = card.Name.AsNonEmptyString() + $" [{card.Serial}]",
                                    Description = $"Type: {card.TypeToString()+ "\n"}" + $"Traits: {card.Traits.Select(t => t.AsNonEmptyString()).ConcatAsString(" - ")}\n" +
                                      $"Effect: {card.Effect.ConcatAsString("\n")}"
                                });

            var serialStringList = serialList.Select(c => c.Serial).ToList();

            var finalTemplateLUA = Encoding.UTF8.GetString(TTSResources.template)
                .Replace("<deck_name_info_placeholder>", $"\"{deck.Name.AsFileNameFriendly()}\"")
                .Replace("<serials_placeholder>", $"\"{JsonConvert.SerializeObject(serialStringList).EscapeQuotes()}\"")
                .Replace("<serial_info_placeholder>", $"\"{JsonConvert.SerializeObject(serialDictionary).EscapeQuotes()}\"")
                ;

            var saveState = JsonConvert.DeserializeObject<SaveState>(Encoding.UTF8.GetString(TTSResources.CustomObject));
            saveState.ObjectStates[0].LuaScript = finalTemplateLUA;

            var nameOfObject = $"Deck Generator ({fileNameFriendlyDeckName})";
            var deckGeneratorPath = resultFolder.Combine($"{nameOfObject}.json");
            deckGeneratorPath.Open(s =>
            {
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(s))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(w, saveState);
                }
            }, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
            var deckGeneratorImage = resultFolder.Combine($"{nameOfObject}.png");
            deckGeneratorImage.Open(s => {
                using (Image img = Image.Load(TTSResources.WeissSchwarzLogo))
                    img.SaveAsPng(s);
            }, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);

            Log.Information($"Done! Relevant Files have been saved in: {resultFolder.FullPath}");

            if (info.OutCommand != "")
            {
                var fullOutCommand = info.OutCommand;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && fullOutCommand.ToLower() == "sharex")
                    fullOutCommand = InstalledApplications.GetApplictionInstallPath("ShareX") + @"ShareX.exe";

                var cmd = $"{fullOutCommand} {deckImagePath.FullPath}";
                Log.Information("Executing {command}", cmd);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = fullOutCommand;
                startInfo.Arguments = $"\"{deckImagePath.FullPath.EscapeQuotes()}\"";
                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                //startInfo.RedirectStandardOutput = true;
                process.StartInfo = startInfo;

                try
                {
                    if (process.Start())
                        Log.Information("Command executed successfully.");
//                        while (!process.HasExited)
//                            Console.WriteLine(await process.StandardOutput.ReadLineAsync());
                } catch (Win32Exception)
                {
                    Log.Warning("Command specified in --out failed; execute it manually.");
                }
            }
        }
    }
}
