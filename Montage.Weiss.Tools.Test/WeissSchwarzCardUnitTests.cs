using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Entities;
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

    }
}
