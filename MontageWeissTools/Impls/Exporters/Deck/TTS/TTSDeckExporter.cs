using Fluent.IO;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore.Migrations;
using Montage.Card.API.Entities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Resources;
using Montage.Weiss.Tools.Resources.TTS;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS
{
    public class TTSDeckExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
    {
        private ILogger Log = Serilog.Log.ForContext<TTSDeckExporter>();

        private (IImageEncoder, IImageFormat) _pngEncoder = (new PngEncoder(), PngFormat.Instance);
        private (IImageEncoder, IImageFormat) _jpegEncoder = (new JpegEncoder(), JpegFormat.Instance);
        private readonly Func<Flurl.Url, CookieSession> _cookieSession;

        public string[] Alias => new [] { "tts", "tabletopsim" };

        public TTSDeckExporter(IContainer ioc)
        {
            _cookieSession = (url) => ioc.GetInstance<GlobalCookieJar>()[url.Root];
        }

        public async Task Export(WeissSchwarzDeck deck, IExportInfo info)
        {
            var count = deck.Ratios.Keys.Count;
            int rows = (int)Math.Ceiling(deck.Count / 10d);

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
                    Log.Information("Loading Images: ({i}/{count}) [{serial}]", i+1, count, p.Serial);
                    return p;
                })
                .SelectAwait(async (wsc) => (card: wsc, stream: await wsc.GetImageStreamAsync(_cookieSession(wsc.Images.Last()))))
                .ToDictionaryAsync(p => p.card, p => PreProcess(Image.Load(p.stream)));

            var (encoder, format) = info.Flags.Any(s => s.ToLower() == "png") == true ? _pngEncoder : _jpegEncoder;
            var newImageFilename = $"deck_{fileNameFriendlyDeckName.ToLower()}.{format.FileExtensions.First()}";
            var deckImagePath = resultFolder.Combine(newImageFilename);

            GenerateDeckImage(info, rows, serialList, imageDictionary, encoder, deckImagePath);

            Log.Information("Generating the Custom Object for TTS...");

            var serialDictionary = deck.Ratios.Keys
                .ToDictionary(card => card.Serial,
                                card => new
                                {
                                    Name = card.Name.AsNonEmptyString() + $" [{card.Serial}][{card.Type.AsShortString()}]",
                                    Description = FormatDescription(card)
                                });

            var serialStringList = serialList.Select(c => c.Serial).ToList();

            var finalTemplateLUA = TTSResources.LuaTemplate
                .Replace("<deck_name_info_placeholder>", $"\"{deck.Name.EscapeQuotes()}\"")
                .Replace("<serials_placeholder>", $"\"{JsonConvert.SerializeObject(serialStringList).EscapeQuotes()}\"")
                .Replace("<serial_info_placeholder>", $"\"{JsonConvert.SerializeObject(serialDictionary).EscapeQuotes()}\"")
                ;

            var finalTemplateUIXML = TTSResources.XMLUITemplate;
            var saveState = JsonConvert.DeserializeObject<SaveState>(Encoding.UTF8.GetString(TTSResources.CustomObject));
            saveState.ObjectStates[0].LuaScript = finalTemplateLUA;
            saveState.ObjectStates[0].XmlUI = finalTemplateUIXML;

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
            deckGeneratorImage.Open(s =>
            {
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
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    Log.Warning("Command specified in --out failed; execute it manually.");
                }
            }

            if (info.Flags?.Contains("sendtcp", StringComparer.CurrentCultureIgnoreCase) ?? false)
               await SendDeckGeneratorJSON("localhost", 39999, saveState);

            static string FormatDescription(WeissSchwarzCard card)
            {
                return $"Type: {card.TypeToString()}\n"
                    + ((card.Type == CardType.Character) ? (
                        $"Traits: {card.Traits.Select(t => t.AsNonEmptyString()).ConcatAsString(" - ")}\n"
                      + $"P/S: {card.Power}P/{card.Soul}S || "
                        ) : "")
                    + ((card.Type != CardType.Climax) ? $"Lv/Co: {card.Level}/{card.Cost}\n" : $"Triggers: {card.Triggers.Select(c => c.ToString()).ConcatAsString(" - ")}\n")
                    + $"{card.Effect.ConcatAsString("\n")}";
            }
        }

        public async Task SendDeckGeneratorJSON(string host, int ttsPort, SaveState saveState)
        {
            Log.Information("Generating a TTS command...");
            var serializedObject = JsonConvert.SerializeObject(saveState.ObjectStates[0]).EscapeQuotes();
            var command = new TTSExternalEditorCommand("-1", 
                $"spawnObjectJSON({{ json = \"{serializedObject}\" }})\n" +
                $"return true"
                );
            var stopSignalGUID = Guid.NewGuid();
            var stopCommand = new TTSExternalEditorCommand("-1", $"sendExternalMessage({{ StopID = \"{stopSignalGUID}\" }})");

            Log.Information("Trying to connect to TTS via {ip:l}:{port}...", host, ttsPort);
            var tcpServer = new TcpListener(System.Net.IPAddress.Loopback, 39998);
            try
            {
                tcpServer.Start();
                var endSignal = WaitForEndSignal();
                Log.Information("Connected. Spawning a Deck Generator in TTS...");
                await SendTTSCommand(host, ttsPort, command);
                await SendTTSCommand(host, ttsPort, stopCommand);
                await endSignal;
            }
            catch (Exception e)
            {
                Log.Warning("Unable to send the Deck Generator directly to TTS; please load the object manually.");
                if (Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                    throw e;
            } finally
            {
                if (tcpServer?.Pending() ?? false)
                    tcpServer.Stop();
            }

            async Task WaitForEndSignal()
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(20));
                var task = Task.Run(async () => await RunAndWaiForTheEndSignal(stopSignalGUID, tcpServer), cts.Token);
                await Task.Yield();
                await task;
            }
        }

        async Task SendTTSCommand(string host, int ttsPort, TTSExternalEditorCommand command)
        {
            using (var tcpClient = new TcpClient(host, ttsPort))
            using (var stream = tcpClient.GetStream())
            using (var writer = new System.IO.StreamWriter(stream))
                try
                {
                    await writer.WriteAsync(JsonConvert.SerializeObject(command));
                    await writer.FlushAsync();
                }
                finally
                {
                    tcpClient?.Close();
                }
        }

        private async Task RunAndWaiForTheEndSignal(Guid stopSignalGUID, TcpListener tcpServer)
        {
            do
            {
                using (var tcpRecieverClient = await tcpServer.AcceptTcpClientAsync())
                using (var recieverStream = tcpRecieverClient.GetStream())
                using (var reader = new System.IO.StreamReader(recieverStream))
                    try
                    {
                        var data = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(data))
                        {
                            Log.Debug($"Recieved Data: {data}");
                            try
                            {
                                dynamic json = JsonConvert.DeserializeObject(data);
                                if (json?.messageID == 4 && json.customMessage?.StopID?.ToString() == stopSignalGUID.ToString())
                                    return;
                            }
                            catch (Exception) { }
                        }
                    }
                    finally
                    {
                        tcpRecieverClient?.Close();
                    }
            } while (true);
        }

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

                    Log.Information("Finished drawing all cards in serial order; saving image...");
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
