using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities.External.DeckLog;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.DeckLog;

[TestClass]
public class PostProcessorTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "API Version Check (DeckLog JP)")]
    public async Task EnsureLatestVersionJP()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.Japanese;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API (JP) version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod(DisplayName = "API Version Test (DeckLog EN)")]
    public async Task EnsureLatestVersionEN()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.English;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API (EN) version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod(DisplayName = "DeckLog API Set List Test (EN)")]
    [TestCategory("Manual")]
    public async Task TestPostProcessor()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progress = NoOpProgress<object>.Instance;

        await new UpdateVerb().Run(ioc, progress, TestContext.CancellationToken);

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469", progress, TestContext.CancellationToken);

        var resultCards = await deckLogPP
            .Process(cards, progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.Contains(
            new Uri("https://en.ws-tcg.com/wordpress/wp-content/images/cardimages/b/bd_en_w03/BD_EN_W03_001.png"), 
            resultCards["BD/EN-W03-001"].Images,
            "The expected URI is missing."
            );
    }

    [TestMethod(DisplayName = "Set List Test (DeckLog API)(HHW)")]
    [TestCategory("Manual")]
    [TestCategory("Bug")]
    public async Task TestPostProcessor2()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progress = NoOpProgress<object>.Instance;

        await new UpdateVerb().Run(ioc, progress, TestContext.CancellationToken);
        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();

        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5c6763677cd9b718cdb87eca", progress, TestContext.CancellationToken);

        var resultCards = await deckLogPP.Process(cards, progress, TestContext.CancellationToken)
            .DistinctBy(c => c.Serial)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.Contains(
            new Uri("https://ws-tcg.com/wordpress/wp-content/images/cardlist/b/bd_w54/bd_w54_008.png"), 
            resultCards["BD/W54-008"].Images,
            "The expected URI is missing."
            );
    }

    [TestMethod(DisplayName = "Set List Test (DeckLog API)(Key All-Stars Clannad)")]
    [TestCategory("Manual")]
    [TestCategory("Bug")]
    [DeploymentItem("Resources/key_all_stars_clannad.txt")]
    public async Task TestPostProcessor3()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progress = NoOpProgress<object>.Instance;

        await new UpdateVerb().Run(ioc, progress, TestContext.CancellationToken);
        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();

        var cards = new Tools.Impls.Parsers.Cards.HeartOfTheCardsURLParser()
            .Parse("./key_all_stars_clannad.txt", progress, TestContext.CancellationToken);
        var resultCards = await deckLogPP.Process(cards, progress, TestContext.CancellationToken)
            .DistinctBy(c => c.Serial)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.Contains(
            new Uri("https://ws-tcg.com/wordpress/wp-content/images/cardlist/k/key_w102/kcl_w102_021s.png"), 
            resultCards["Kcl/W102-021S"].Images,
            "The expected URI is missing."
            );
    }


    [TestMethod(DisplayName = "Set List Test (DeckLog API Set)(Nanoha)")]
    [TestCategory("Manual")]
    [TestCategory("Bug")]
    public async Task TestPostProcessor4()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progress = NoOpProgress<object>.Instance;

        await new UpdateVerb().Run(ioc, progress, TestContext.CancellationToken);
        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();

        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=6862dbb3abea99e627d998b4", progress, TestContext.CancellationToken);
        var resultCards = await deckLogPP.Process(cards, progress, TestContext.CancellationToken)
            .DistinctBy(c => c.Serial)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.Contains(
            new Uri("https://ws-tcg.com/wordpress/wp-content/images/cardlist/n/nta_we48/nta_we48_25.png"), 
            resultCards["NTA/WE48-25"].Images,
            "The expected URI is missing."
            );
    }
}