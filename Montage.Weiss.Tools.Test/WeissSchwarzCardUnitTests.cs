using Flurl.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Test.Commons;
using Montage.Weiss.Tools.Utilities;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.ENWS
{
    [TestClass]
    public class WeissSchwarzCardUnitTests
    {
        [TestMethod("Serial Parser Test")]
        public async Task TestSerialParser()
        {
            var serials = new[] { 
               "LSS/W69-054",
               "BD/EN-W03-026BDR",
               "CCS/BSF2019-02"
            };
            var expectedOutput = new (string NeoStandardCode, string ReleaseID, string SetID)[]
            {
                ("LSS", "W69", "054"),
                ("BD", "EN-W03", "026BDR"),
                ("CCS", "BSF2019", "02")
            };

            for (int i = 0; i < serials.Length; i++)
            {
                var (NeoStandardCode, ReleaseID, SetID) = WeissSchwarzCard.ParseSerial(serials[i]);
                var tuple = (NeoStandardCode, ReleaseID, SetID);
                Assert.IsTrue(expectedOutput[i] == tuple);
            }

            await Task.CompletedTask;
        }

        [TestMethod]
        [Ignore]
        public void TestShareX()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            var Log = Serilog.Log.Logger.ForContext<WeissSchwarzCardUnitTests>();
            Log.Information(InstalledApplications.GetApplictionInstallPath("ShareX"));
        }

        [TestMethod]
        [Ignore]
        public async Task TestEncoreDecksGifsAsync()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            var Log = Serilog.Log.Logger.ForContext<WeissSchwarzCardUnitTests>();
            var url = new Uri("https://www.encoredecks.com/images/JP/W69/039.gif");
            using (var img = Image.Load(await url.WithImageHeaders().GetStreamAsync()))
            {
                Log.Information("Image Loaded.");
            }
        }

    }
}
