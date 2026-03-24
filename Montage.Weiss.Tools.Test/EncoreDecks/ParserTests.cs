using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Test.Commons;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.EncoreDecks;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Parse Test (Batman Ninja / BDML / LB Anime)")]
    public async Task FullTestRun()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var batmanNinja = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards", progress, TestContext.CancellationToken);
        Assert.IsTrue(await batmanNinja.AnyAsync(c => c.Serial == "BNJ/BCS2019-02", TestContext.CancellationToken));

        var bdml = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469", progress, TestContext.CancellationToken);
        Assert.IsTrue(await bdml.AnyAsync(c => c.Serial == "BD/EN-W03-007" || c.Effect.Length < 1, TestContext.CancellationToken), "BD/EN-W03 might have not been parsed correctly; do check.");

        var lbAnime = new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5d6d4e147cd9b718cd3a0d40", progress, TestContext.CancellationToken);
        Log.Information("LB Anime Set has been parsed successfully. This was a set that is not translated when this Unit Test was created.");
        var serialChecklist = new[] { "LB/W21-078", "LB/W21-065" };
        Assert.IsTrue(await bdml.AnyAsync(c => !serialChecklist.Contains(c.Serial), TestContext.CancellationToken), "LB/W21 might have not been parsed correctly; do check.");
    }

    [TestMethod(DisplayName = "Batman Ninja Trait Test")]
    public async Task TraitTestRun()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var batmanNinja = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsEmpty(batmanNinja["BNJ/SX01-T07"].Traits);
        Assert.IsEmpty(batmanNinja["BNJ/SX01-A13"].Traits);
        Assert.IsTrue(batmanNinja["BNJ/SX01-078b"].Traits.Select(mls => mls.EN).All(trait => new[] { "Sengoku", "Weapon" }.Contains(trait)));
    }

    [TestMethod(DisplayName = "Climax Trigger Test (BDML)")]
    public async Task TestClimaxTriggers()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var bdml = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);
        
        Assert.AreEqual(2, bdml["BD/EN-W03-125"].Triggers.Length);
        Assert.IsTrue(bdml["BD/EN-W03-125"].Triggers.Contains(Trigger.Gate));
        Assert.IsTrue(bdml["BD/EN-W03-126"].Triggers.Contains(Trigger.Book));
    }

    [TestMethod(DisplayName = "Serial Test (Prisma Illya)")]
    public async Task TestPISerials()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var prismaIllyaEN = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/?page=1&set=5c7b101d7cd9b718cdbd085e", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsNotNull(prismaIllyaEN["PI/EN-S04-E038"]);
    }

    [TestMethod(DisplayName = "Akiba Test (Prisma Illya)")]
    public async Task TestPIAkiba()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var prismaIllyaHertz = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5d9a1ccc7cd9b718cd5b2200/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(prismaIllyaHertz["PI/S40-038"].Name.EN == null);
    }

    [TestMethod(DisplayName = "Trait Test (Akiba)")]
    public async Task TestAkibaNullTraits()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var set = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5f7e38ea5f277795ebad6eec/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsEmpty(set["DC/W81-007"].Traits);

        set = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5ea363565f277795eba7fea8/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsEmpty(set["KS/W76-025"].Traits);
    }

    [TestMethod(DisplayName = "Color Test (Akiba)(Edge Case)")]
    public async Task TestAkibaColors()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var set = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5f7e38ea5f277795ebad6eec/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(CardColor.Yellow, set["DC4/W81-P06"].Color);
    }

    [TestMethod(DisplayName = "Trait Test (Yosuke Bias)")]
    public async Task TestYosukeBiasTraits()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progress = NoOpProgress<SetParserProgressReport>.Instance;

        var set = await new Tools.Impls.Parsers.Cards.EncoreDecksParser()
            .Parse("https://www.encoredecks.com/api/series/5c7b0f9a7cd9b718cdbd082c/cards", progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(set
            .Where(c => c.Value.Name?.EN?.Contains("Yusuke") ?? false)
            .All(c => c.Value?.Traits?.Any(t => t.EN == "Junes") ?? false)
            );
    }
}