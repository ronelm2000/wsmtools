using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.PostProcessors
{
    [TestClass]

    public class DeckLogTests
    {
        [TestMethod("DeckLog API Version Test")]
        [DeploymentItem("Resources/deck_date_a_live.json")]
        public async Task EnsureLatestVersion()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
            Assert.IsTrue(await deckLogPP.GetLatestVersion() == deckLogPP.Settings.Version, "DeckLog API version is outdated; need to check for compatibility.");
        }
    }
}
