using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.PostProcessors;
using Montage.Weiss.Tools.Entities.External.DeckLog;
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
    public async Task EnsureLatestVersionJP()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.Japanese;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API (JP) version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod("DeckLog API Version Test (EN)")]
    public async Task EnsureLatestVersionEN()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var settings = DeckLogSettings.English;
        var latestVersion = await deckLogPP.GetLatestVersion(settings);
        Assert.IsTrue(latestVersion == settings.Version, $"DeckLog API (EN) version is outdated; latest version is {latestVersion} need to check for compatibility.");
    }

    [TestMethod("DeckLog API Set List Test (EN)")]
    [TestCategory("Manual")]
    public async Task TestPostProcessor()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progress1 = NoOpProgress<SetParserProgressReport>.Instance;
        var progress2 = NoOpProgress<PostProcessorProgressReport>.Instance;
        var progress3 = NoOpProgress<CommandProgressReport>.Instance;
        var ct = CancellationToken.None;

        await new UpdateVerb().Run(ioc, progress3, ct);

        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();
        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469", progress1, ct);

        var resultCards = await deckLogPP.Process(cards, progress2, ct)
            .ToDictionaryAsync(c => c.Serial);

        Assert.IsTrue(resultCards["BD/EN-W03-001"]
            .Images
            .Contains(new Uri("https://en.ws-tcg.com/wp/wp-content/images/cardimages/b/bd_en_w03/BD_EN_W03_001.png")),
            "The expected URI is missing."
            );
    }

    [TestMethod("DeckLog API Set List Test - Reported Bug for HHW")]
    [TestCategory("Manual")]
    public async Task TestPostProcessor2()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progress1 = NoOpProgress<SetParserProgressReport>.Instance;
        var progress2 = NoOpProgress<PostProcessorProgressReport>.Instance;
        var progress3 = NoOpProgress<CommandProgressReport>.Instance;
        var ct = CancellationToken.None;

        await new UpdateVerb().Run(ioc, progress3, ct);
        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();

        var cards = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5c6763677cd9b718cdb87eca", progress1, ct);

        var resultCards = await deckLogPP.Process(cards, progress2, ct)
            .Distinct(c => c.Serial)
            .ToDictionaryAsync(c => c.Serial);

        Assert.IsTrue(resultCards["BD/W54-008"]
            .Images
            .Contains(new Uri("https://ws-tcg.com/wordpress/wp-content/images/cardlist/b/bd_w54/bd_w54_008.png")),
            "The expected URI is missing."
            );
    }

    [TestMethod("DeckLog API Set List Test - Bug for Key All-Stars Clannad Only")]
    [TestCategory("Manual")]
    [DeploymentItem("Resources/key_all_stars_clannad.txt")]
    public async Task TestPostProcessor3()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progress1 = NoOpProgress<SetParserProgressReport>.Instance;
        var progress2 = NoOpProgress<PostProcessorProgressReport>.Instance;
        var progress3 = NoOpProgress<CommandProgressReport>.Instance;
        var ct = CancellationToken.None;

        await new UpdateVerb().Run(ioc, progress3, ct);
        var deckLogPP = ioc.GetInstance<DeckLogPostProcessor>();

        var cards = new Tools.Impls.Parsers.Cards.HeartOfTheCardsURLParser()
            .Parse("./key_all_stars_clannad.txt", progress1, ct);
        var resultCards = await deckLogPP.Process(cards, progress2, ct)
            .Distinct(c => c.Serial)
            .ToDictionaryAsync(c => c.Serial);

        Assert.IsTrue(resultCards["Kcl/W102-021S"]
            .Images
            .Contains(new Uri("https://ws-tcg.com/wordpress/wp-content/images/cardlist/k/key_w102/kcl_w102_021s.png")),
            "The expected URI is missing."
            );
    }
}