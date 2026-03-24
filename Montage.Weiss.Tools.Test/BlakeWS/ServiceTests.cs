using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Test.Commons;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.BlakeWS;
[TestClass]
public class ServiceTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Import Deck Test (Get)")]
    [TestCategory("Manual")]
    [OSCondition(OperatingSystems.Windows)]
    public void GetTestImportDeck()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var wsblakeSrvc = ioc.GetInstance<WeissSchwarzBlakeUnityService>();
        var data = wsblakeSrvc!.GetExportDeckData();
        Assert.IsNotNull(data);
    }

    [TestMethod(DisplayName = "Import Deck Test (Insert)(Regedit)")]
    [TestCategory("Manual")]
    [DeploymentItem("Resources/deck_date_a_live.json")]
    [OSCondition(OperatingSystems.Windows)]
    public void TestInsertImportDeckToBlakeWSS()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var wsblakeSrvc = ioc.GetInstance<WeissSchwarzBlakeUnityService>();

        var dataToImport = "P5/S45-E109|P5/S45-E109|P5/S45-E109|P5/S45-E009|P5/S45-E009|P5/S45-E003|P5/S45-E003|P5/S45-E003|P5/S45-E003|P5/S45-E008|P5/S45-E011|P5/S45-E011|P5/S45-E011|P5/S45-E011|P5/S45-E015|P5/S45-E033|P5/S45-E033|P5/S45-TE03|P5/S45-E017|P5/S45-E017|P5/S45-E017|P5/S45-E016|P5/S45-E016|P5/S45-E016|P5/S45-E016|P5/S45-E021|P5/S45-E021|P5/S45-E021|P5/S45-E006|P5/S45-E006|P5/S45-E006|P5/S45-E006|P5/S45-E101|P5/S45-E012|P5/S45-E012|P5/S45-E001|P5/S45-E001|P5/S45-E001|P5/S45-E001|P5/S45-E002|P5/S45-E002|P5/S45-E002|P5/S45-E024|P5/S45-E024|P5/S45-E024|P5/S45-E024|P5/S45-E023|P5/S45-E023|P5/S45-E023|P5/S45-E023|\0";
        var timeToImport = DateTime.Now;
        timeToImport = new DateTime(timeToImport.Year, timeToImport.Month, timeToImport.Day, timeToImport.Hour, timeToImport.Minute, 0, timeToImport.Kind);

        wsblakeSrvc.ExportDeckData(dataToImport);
        wsblakeSrvc.ExportDeckDate(timeToImport);

        var importedData = wsblakeSrvc!.GetExportDeckData();
        DateTime? importedDateTime = wsblakeSrvc.GetExportDate();

        Assert.AreEqual(dataToImport, importedData, "Data was not imported correctly.");
        Assert.IsTrue(importedDateTime?.Equals(timeToImport) ?? false, "Timestamp was not imported correctly.");
    }

    [TestMethod(DisplayName = "Export Deck Test (Blake WS Format)")]
    [DeploymentItem("Resources/deck_Lucky_Happy_Smile_Yay_v3.json")]
    [DeploymentItem("Resources/test_deck_results.bws.txt")]
    [TestCategory("Manual")]
    public async Task TestExportToBlakeWSFormat()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        await new FetchVerb
        {
            RIDsOrSerials = new[] { "W73", "W63", "W54" }
        }.Run(ioc, NoOpProgress<CommandProgressReport>.Instance, TestContext.CancellationToken);

        await new ExportVerb
        {
            Source = "./deck_Lucky_Happy_Smile_Yay_v3.json",
            Exporter = "bws"
        }.Run(ioc, NoOpProgress<CommandProgressReport>.Instance, TestContext.CancellationToken);

        var resourceStream = await Path.Get("./test_deck_results.bws.txt").ReadBytesAsync(TestContext.CancellationToken);
        var resultStream = await Path.Get("./Export/Lucky_Happy_Smile_Yay_v3.bws.txt").ReadBytesAsync(TestContext.CancellationToken);
        var hasher = SHA512.Create();
        var resourceHash = hasher.ComputeHash(resourceStream);
        var resultHash = hasher.ComputeHash(resultStream);

        var resourceString = ASCIIEncoding.ASCII.GetString(resourceStream);
        var resultString = ASCIIEncoding.ASCII.GetString(resultStream);

        Log.Information("Expected: {s}", resourceString);
        Log.Information("Obtained: {s}", resultString);

        Assert.IsTrue(resourceHash?.SequenceEqual(resultHash) ?? false);
    }
}
