using Fluent.IO;
using Flurl.Http;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Utilities;
using Montage.Weiss.Tools.Resources.TTS;
using Montage.Weiss.Tools.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class TTSDeckExporter : IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
{
    private ILogger Log = Serilog.Log.ForContext<TTSDeckExporter>();

    private (IImageEncoder, IImageFormat) _pngEncoder = (new PngEncoder(), PngFormat.Instance);
    private (IImageEncoder, IImageFormat) _jpegEncoder = (new JpegEncoder(), JpegFormat.Instance);
    private readonly Func<Flurl.Url, Task<CookieJar>> _cookieSession;
    private readonly Func<LocalDeckImageExporter> _localDeckImageExporter;

    private readonly Uri defaultURI = new Uri("https://www.google.com/");

    public string[] Alias => new [] { "tts", "tabletopsim" };

    public TTSDeckExporter(IContainer ioc)
    {
        _cookieSession = (url) => ioc.GetInstance<GlobalCookieJar>().FindOrCreate(url.Root);
        _localDeckImageExporter = () => ioc.GetInstance<LocalDeckImageExporter>();
    }

    public async Task Export(WeissSchwarzDeck deck, IExportInfo info, CancellationToken cancellationToken = default)
    {
        var count = deck.Ratios.Keys.Count;
        int rows = (int)Math.Ceiling(deck.Count / 10d);

        if (count < 1)
        {
            var fprogress = info.Progress;
            var freport = DeckExportProgressReport.Cancelling(deck.Name, "TTS", "having 0 cards in deck");
            fprogress.Report(freport);
            return;
        }

        var progress = info.Progress;
        var report = DeckExportProgressReport.Starting(deck.Name, "TTS");
        progress.Report(report);

        var serialList = deck.Ratios.Keys
            .OrderBy(c => c.Serial)
            .SelectMany(c => Enumerable.Range(0, deck.Ratios[c]).Select(i => c))
            .ToList();

        var resultFolder = Path.CreateDirectory(info.Destination);

        var fileNameFriendlyDeckName = deck.Name.AsFriendlyToTabletopSimulator();

        var imageDictionary = await deck.Ratios.Keys
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
                stream: await wsc.GetImageStreamAsync(await _cookieSession(wsc.Images.LastOrDefault(defaultURI)), ct))
            )
            .ToDictionaryAwaitWithCancellationAsync(
                async (p, ct) => await ValueTask.FromResult(p.card),
                async (p, ct) => PreProcess(await Image.LoadAsync(p.stream, ct)),
                cancellationToken
                );

        var (encoder, format) = info.Flags.Any(s => s.ToLower() == "png") == true ? _pngEncoder : _jpegEncoder;
        var newImageFilename = $"deck_{fileNameFriendlyDeckName.ToLower()}.{format.FileExtensions.First()}";
        var deckImagePath = resultFolder.Combine(newImageFilename);

        //GenerateDeckImage(info, rows, serialList, imageDictionary, encoder, deckImagePath);
        await _localDeckImageExporter().GenerateDeckImage(info, new(rows, serialList, imageDictionary, encoder, deckImagePath, progress.From().Translate<DeckExportProgressReport>(r => r.AsRatio<DeckExportProgressReport, DeckExportProgressReport>(10, 0.40f)), report, cancellationToken));

        Log.Information("Generating the Custom Object for TTS...");
        report = report with { Exporter = "TTS", Percentage = 55, ReportMessage = new() { EN = "Generating Tabletop Simulator Custom Object..."}};
        progress.Report(report);

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
            .Replace("<serials_placeholder>", $"\"{JsonSerializer.Serialize(serialStringList).EscapeQuotes()}\"")
            .Replace("<serial_info_placeholder>", $"\"{JsonSerializer.Serialize(serialDictionary).EscapeQuotes()}\"")
            ;

        var finalTemplateUIXML = TTSResources.XMLUITemplate;
        var saveState = JsonSerializer.Deserialize<SaveState>(Encoding.UTF8.GetString(TTSResources.CustomObject), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PreferredObjectCreationHandling = System.Text.Json.Serialization.JsonObjectCreationHandling.Populate,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
            ReadCommentHandling = JsonCommentHandling.Skip
        }) ?? throw new InvalidOperationException();
        saveState.ObjectStates[0].LuaScript = finalTemplateLUA;
        saveState.ObjectStates[0].XmlUI = finalTemplateUIXML;

        var nameOfObject = $"Deck Generator ({deck.Name})";
        var deckGeneratorPath = resultFolder.Combine($"{nameOfObject.AsFileNameFriendly()}.json");

        //TODO: Add more progress logs here.

        await using (var deckGenStream = deckGeneratorPath.GetOpenWriteStream())
        {
            await JsonSerializer.SerializeAsync(deckGenStream, saveState, options: new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        var deckGeneratorImage = resultFolder.Combine($"{nameOfObject.AsFileNameFriendly()}.png");
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
                fullOutCommand = InstalledApplications.GetApplicationInstallPath("ShareX") + @"ShareX.exe";

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
                    $"Traits: {card.Traits.Select(t => t.TraitString).ConcatAsString(" - ")}\n"
                  + $"P/S: {card.Power}P/{card.Soul}S || "
                    ) : "")
                + ((card.Type != CardType.Climax) ? $"Lv/Co: {card.Level}/{card.Cost}\n" : $"Triggers: {card.Triggers.Select(c => c.ToString()).ConcatAsString(" - ")}\n")
                + $"{card.Effect.ConcatAsString("\n")}";
        }
    }

    public async Task SendDeckGeneratorJSON(string host, int ttsPort, SaveState saveState)
    {
        Log.Information("Generating a TTS command...");
        var serializedObject = JsonSerializer.Serialize(
            saveState.ObjectStates[0], 
            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
            ).EscapeQuotes();
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
        catch (Exception) when (!Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
        {                       
            Log.Warning("Unable to send the Deck Generator directly to TTS; please load the object manually.");
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
                await writer.WriteAsync(JsonSerializer.Serialize(command));
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
                            JsonNode? json = JsonSerializer.Deserialize<JsonNode>(data);
                            if (json?["messageID"]?.GetValue<int>() == 4 && json?["customMessage"]?["StopID"]?.ToString() == stopSignalGUID.ToString())
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
}
