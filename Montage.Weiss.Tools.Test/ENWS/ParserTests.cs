using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
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
        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E018";
        var dict = await Global.Container.GetInstance<EnglishWSURLParser>()!
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
        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/list/?cardno=CCS/BSF2019-02";
        var list = await Global.Container.GetInstance<EnglishWSURLParser>()!
            .Parse(url, progressReporter, TestContext.CancellationToken)
            .ToListAsync(TestContext.CancellationToken);

        Log.Information("Cards Obtained: {length}", list.Count);

        foreach (var card in list)
            Log.Information("Card: {@card}", card.Serial);
    }

    [TestMethod(DisplayName = "EN WS Parser - SearchResults URL Test")]
    public async Task TestSearchResultsUrl()
    {
        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/searchresults/?expansion=280&view=text";
        var dict = await Global.Container.GetInstance<EnglishWSURLParser>()!
            .Parse(url, progressReporter, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, c => c, cancellationToken: TestContext.CancellationToken);

        Log.Information("Cards Obtained: {length}", dict.Keys.Count);
        Assert.IsTrue(dict.Keys.Count > 0, "No cards parsed from searchresults URL.");
    }

    [TestMethod(DisplayName = "EN WS Parser - SearchResults Compatibility Test")]
    public async Task TestSearchResultsCompatibility()
    {
        var parser = Global.Container.GetInstance<EnglishWSURLParser>()!;
        var parseInfo = new TestParseInfo
        {
            URI = "https://en.ws-tcg.com/cardlist/searchresults/?expansion=280&view=text"
        };

        var result = await parser.IsCompatible(parseInfo);
        Assert.IsTrue(result, "searchresults URL should be compatible.");
    }

    [TestMethod(DisplayName = "EN WS Parser - SearchResults Missing Expansion Test")]
    public async Task TestSearchResultsMissingExpansion()
    {
        var url = "https://en.ws-tcg.com/cardlist/searchresults/?view=text";
        var parser = Global.Container.GetInstance<EnglishWSURLParser>()!;
        var parseInfo = new TestParseInfo
        {
            URI = url
        };

        var result = await parser.IsCompatible(parseInfo);
        Assert.IsFalse(result, "URL without expansion should not be compatible.");
    }

    private class TestParseInfo : Montage.Card.API.Entities.IParseInfo
    {
        public string URI { get; set; } = string.Empty;
        public IEnumerable<string> ParserHints { get; set; } = [];
    }

    public static void AssertExistsInDictionary(Dictionary<string, WeissSchwarzCard> dict, string serial)
    {
        Assert.IsTrue(dict.ContainsKey(serial), $"Could not find {serial} in output.");
    }
}
