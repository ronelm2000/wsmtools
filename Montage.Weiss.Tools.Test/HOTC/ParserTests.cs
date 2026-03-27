using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.HOTC;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Full Integration (Local Text File) (HOTC)")]
    [DeploymentItem("Resources/shiyoko_prs_hotc.txt")]
    public async Task FullTestRun()
    {
        await new ParseVerb()
        {
            URI = "./shiyoko_prs_hotc.txt",
            ParserHints = ["hotc"]
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);
    }

    [TestMethod(DisplayName = "HOTC Parser Trait Test")]
    public async Task TestTraitHandling()
    {
        var lba = await new HeartOfTheCardsURLParser()
            .Parse("https://www.heartofthecards.com/translations/little_busters!_anime_booster_pack.html", Global.MockProgress, TestContext.CancellationToken) //
            .ToDictionaryAsync(c => c.Serial, cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(lba["LB/W21-046"].Traits.Count == 1, $"LB/W21-046 has an invalid amount of traits: {lba["LB/W21-046"].Traits.Count}");
        Assert.IsTrue(lba["LB/W21-065"].Traits.Count == 0, $"LB/W21-065has an invalid amount of traits: {lba["LB/W21-065"].Traits.Count}");
    }
}
