using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.DeckLog;

[TestClass]
public class PostProcessorTests
{
    [TestMethod("DeckLog API Version Test (JP)")]
    [DeploymentItem("Resources/deck_date_a_live.json")]
    public async Task EnsureLatestVersionJP()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.Japanese;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod("DeckLog API Version Test (JP)")]
    [DeploymentItem("Resources/deck_date_a_live.json")]
    public async Task EnsureLatestVersionEN()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.English;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod("DeckLog API Set List Test")]
    [TestCategory("Manual")]
    public async Task TestPostProcessor()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progress1 = NoOpProgress<SetParserProgressReport>.Instance;
        var progress2 = NoOpProgress<PostProcessorProgressReport>.Instance;
        var ct = CancellationToken.None;

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469", progress1, ct);

        var resultCards = await deckLogPP.Process(cards, progress2, ct)
            .ToDictionaryAsync(c => c.Serial);

        Assert.IsTrue(resultCards["BD/EN-W03-001"].Images.Contains(new Uri("https://en.ws-tcg.com/wp/wp-content/images/cardimages/b/bd_en_w03/BD_EN_W03_001.png")), "The expected URI is incorrect.");
    }
}