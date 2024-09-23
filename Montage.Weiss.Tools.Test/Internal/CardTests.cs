using Flurl.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;
using Montage.Weiss.Tools.Test.Commons;
using Montage.Weiss.Tools.Utilities;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Montage.Weiss.Tools.Test.Internal;

namespace Montage.Weiss.Tools.Test.Internal;

[TestClass]
public class CardTests
{
    [TestMethod("Serial Parser Test")]
    public async Task TestSerialParser()
    {
        var serials = new[] { 
           "LSS/W69-054",
           "BD/EN-W03-026BDR",
           "CCS/BSF2019-02",
           "RANDOM"
        };
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        var expectedOutput = new (string NeoStandardCode, string ReleaseID, string SetID)?[]
        {
            ("LSS", "W69", "054"),
            ("BD", "EN-W03", "026BDR"),
            ("CCS", "BSF2019", "02"),
            (null,null,null)
        };
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        for (int i = 0; i < serials.Length; i++)
        {
            var (NeoStandardCode, ReleaseID, SetID) = WeissSchwarzCard.ParseSerial(serials[i]);
            var tuple = (NeoStandardCode, ReleaseID, SetID);
            Assert.IsTrue(expectedOutput[i] == tuple);
        }

        await Task.CompletedTask;
    }
    
    [TestMethod("Foil Removal Serial Test")]
    public async Task TestFoilRemovalSerial()
    {
        var serials = new[] {
           "PI/SE36-40OFR",
           "BD/EN-W03-026BDR",
           "CCS/BSF2019-02"
        };

        var expectedOutputs = new[] {
           "PI/SE36-40",
           "BD/EN-W03-026",
           "CCS/BSF2019-02"
        };

        for (int i = 0; i < serials.Length; i++)
        {
            var output = WeissSchwarzCard.RemoveFoil(serials[i]);
            Assert.IsTrue(expectedOutputs[i] == output, $"Input: {serials[i]} / Expected Output: {expectedOutputs[i]} / Actual Output: {output}");
        }

        await Task.CompletedTask;
    }

    [TestMethod("English Set Test")]
    public async Task TestEnglishSetTypes()
    {
        var serials = new[] {
           "LSS/W69-E054",
           "BD/EN-W03-026BDR",
           "CCS/BSF2019-02",
           "NK/W30-E076",
           "BNJ/SX01-T17SP",
           "LSS/W69-054"
        };
        var expectedOutput = new EnglishSetType?[]
        {
            EnglishSetType.JapaneseImport,
            EnglishSetType.EnglishEdition,
            EnglishSetType.EnglishOriginal,
            EnglishSetType.EnglishEdition,
            EnglishSetType.EnglishOriginal,
            null
        };

        for (int i = 0; i < serials.Length; i++)
        {
            var setType = WeissSchwarzCard.GetEnglishSetType(serials[i]);
            Assert.IsTrue(expectedOutput[i] == setType);
        }
        await Task.CompletedTask;
    }

    [TestMethod]
    [TestCategory("Manual")]
    public void TestShareX()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var Log = Serilog.Log.Logger.ForContext<CardTests>();
        Log.Information(InstalledApplications.GetApplicationInstallPath("ShareX"));
    }

    [TestMethod]
    [TestCategory("Manual")]
    public async Task TestEncoreDecksGifsAsync()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var Log = Serilog.Log.Logger.ForContext<CardTests>();
        var url = new Uri("https://www.encoredecks.com/images/JP/W69/039.gif");
        using (var img = Image.Load(await url.WithImageHeaders().GetStreamAsync()))
        {
            Log.Information("Image Loaded.");
        }
    }

    [TestMethod]
    [Ignore]
    public async Task TestTTSCommand(Task<string?> task)
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var Log = Serilog.Log.Logger.ForContext<CardTests>();

        var host = "localhost";
        var port = 39999;

        //var command = new TTSExternalEditorCommand("-1", "spawnObject({ type = \"rpg_BEAR\" })");
        var command = new TTSExternalEditorCommand("-1", "spawnObject ({ type = \"rpg_BEAR\" })");


        Log.Information("Trying to connect to TTS via {ip}:{port}...", host, port);
        using (var tcpClient = new TcpClient(host, port))
        using (var stream = tcpClient.GetStream())
        using (var writer = new System.IO.StreamWriter(stream))
        using (var reader = new System.IO.StreamReader(stream))
        {   
            var json = JsonConvert.SerializeObject(command);
            Log.Information($"Running: {json}");
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            string response = await (reader?.ReadLineAsync() ?? Task.FromResult<string?>(""))! ?? "";
            Log.Information("Logging Response: {response}", response);
        }
    }
}
