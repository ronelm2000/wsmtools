using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.BlakeWS;
[TestClass]
public class ServiceTests
{
    [TestMethod("Get Import Deck Test")]
    [TestCategory("Manual")]
    public void GetTestImportDeck()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Assert.Inconclusive("Test was not run on Windows and is unsupported in other platforms.");

        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var wsblakeSrvc = ioc.GetInstance<WeissSchwarzBlakeUnityService>();

        string data = wsblakeSrvc.GetExportDeckData();
        Assert.IsNotNull(data);
    }

    [TestMethod("Insert Import Deck Test")]
    [TestCategory("Manual")]
    public void InsertImportDeck()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Assert.Inconclusive("Test was not run on Windows and is unsupported in other platforms.");

        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var wsblakeSrvc = ioc.GetInstance<WeissSchwarzBlakeUnityService>();

        string dataToImport = "P5/S45-E109|P5/S45-E109|P5/S45-E109|P5/S45-E009|P5/S45-E009|P5/S45-E003|P5/S45-E003|P5/S45-E003|P5/S45-E003|P5/S45-E008|P5/S45-E011|P5/S45-E011|P5/S45-E011|P5/S45-E011|P5/S45-E015|P5/S45-E033|P5/S45-E033|P5/S45-TE03|P5/S45-E017|P5/S45-E017|P5/S45-E017|P5/S45-E016|P5/S45-E016|P5/S45-E016|P5/S45-E016|P5/S45-E021|P5/S45-E021|P5/S45-E021|P5/S45-E006|P5/S45-E006|P5/S45-E006|P5/S45-E006|P5/S45-E101|P5/S45-E012|P5/S45-E012|P5/S45-E001|P5/S45-E001|P5/S45-E001|P5/S45-E001|P5/S45-E002|P5/S45-E002|P5/S45-E002|P5/S45-E024|P5/S45-E024|P5/S45-E024|P5/S45-E024|P5/S45-E023|P5/S45-E023|P5/S45-E023|P5/S45-E023|\0";
        DateTime timeToImport = DateTime.Now;
        timeToImport = new DateTime(timeToImport.Year, timeToImport.Month, timeToImport.Day, timeToImport.Hour, timeToImport.Minute, 0, timeToImport.Kind);

        wsblakeSrvc.ExportDeckData(dataToImport);
        wsblakeSrvc.ExportDeckDate(DateTime.Now);

        string importedData = wsblakeSrvc.GetExportDeckData();
        DateTime? importedDateTime = wsblakeSrvc.GetExportDate();

        Assert.IsTrue(importedData == dataToImport, "Data was not imported correctly.");
        Assert.IsTrue(importedDateTime?.Equals(timeToImport) ?? false, "Timestamp was not imported correctly.");
    }
}
