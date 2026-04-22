using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.JPWS;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "JP WS Parser Compatibility Test")]
    public async Task TestCompatibility()
    {
        var parser = new JapaneseWSURLParser();

        Assert.IsTrue(await parser.IsCompatible(new ParseVerb
        {
            URI = "https://ws-tcg.com/cardlist/search/?expansion=548"
        }));

        Assert.IsFalse(await parser.IsCompatible(new ParseVerb
        {
            URI = "https://ws-tcg.com/cardlist/list/?cardno=BD/W54-008"
        }));
    }

    [TestMethod(DisplayName = "JP WS Parser Live Parse Test")]
    public async Task TestParser()
    {
        var progress = Global.MockProgress;
        var url = "https://ws-tcg.com/cardlist/search/?expansion=548";

        var cards = await new JapaneseWSURLParser()
            .Parse(url, progress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(232, cards.Count);
        Assert.IsTrue(cards.ContainsKey("SMP/W137-002"));
        Assert.IsTrue(cards.ContainsKey("SMP/W137-006S"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(cards["SMP/W137-002"].Name.JP));
        Assert.IsTrue(cards["SMP/W137-002"].Effect.Any(line => line.Any(c => c > 127)));
    }
}
