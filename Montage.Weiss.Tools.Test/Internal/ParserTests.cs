using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Internal;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "WS Custom Card Parser Test")]
    [DeploymentItem("Resources")]
    public async Task TestParser()
    {
        var filePath = "./machikado_mazoku.ws-set";
        var dict = await new InternalSetParser()
            .Parse(filePath, Global.MockProgress, TestContext.CancellationToken)
            .ToDictionaryAsync(c => c.Serial, c => c, cancellationToken: TestContext.CancellationToken);

        Log.Information("Cards Obtained: {length}", dict.Count);
        foreach (var card in dict.Values)
            Log.Information("Card: {@card}", card.Serial);

        void serialAssertions(string serial) => Assert.IsTrue(dict.ContainsKey(serial), $"Could not find {serial} in output.");
        serialAssertions("MKM/WC001-E095");
        serialAssertions("MKM/WC001-E100");
        serialAssertions("MKM/WC001-PE01");
    }
}
