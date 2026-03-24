using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Test.Commons;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.ENWS;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "EN WS Parser Test")]
    public async Task TestParser()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E018";
        var dict = await ioc.GetInstance<EnglishWSURLParser>()!
            .Parse(url, progressReporter, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, c => c, cancellationToken: TestContext.CancellationToken);

        Log.Information("Cards Obtained: {length}", dict.Keys.Count);

        AssertExistsInDictionary(dict, "FS/S36-PE01");
        AssertExistsInDictionary(dict, "FS/S36-E012");

        // This is now only obtained from PR Cards [Schwarz Side]
        // AssertExistsInDictionary(dict, "FS/S36-PE02");
    }

    [TestMethod(DisplayName = "EN WS Parser Test - CCS WX")]
    [TestCategory("Manual")]
    public async Task TestParserCCSWX()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();

        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/list/?cardno=CCS/BSF2019-02";
        var list = await ioc.GetInstance<EnglishWSURLParser>()!
            .Parse(url, progressReporter, TestContext.CancellationToken)
            .ToListAsync(TestContext.CancellationToken);
        
        Log.Information("Cards Obtained: {length}", list.Count);
        
        foreach (var card in list)
            Log.Information("Card: {@card}", card.Serial);
    }

    public static void AssertExistsInDictionary(Dictionary<string, WeissSchwarzCard> dict, string serial)
    {
        Assert.IsTrue(dict.ContainsKey(serial), $"Could not find {serial} in output.");
    }
}
