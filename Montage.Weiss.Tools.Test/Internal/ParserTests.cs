using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Test.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Internal;

[TestClass]
public class ParserTests
{
    [TestMethod("WS Custom Card Parser Test")]
    [DeploymentItem("Resources")]
    public async Task TestParser()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var progressReporter = NoOpProgress<object>.Instance;

        var filePath = "./machikado_mazoku.ws-set";
        var dict = await new InternalSetParser().Parse(filePath, progressReporter, CancellationToken.None)
            .ToDictionaryAsync(
                c => c.Serial,
                c => c
            );
        Log.Information("Cards Obtained: {length}", dict.Count);
        foreach (var card in dict.Values)
            Log.Information("Card: {@card}", card.Serial);

        Action<string> serialAssertions = serial => Assert.IsTrue(dict.ContainsKey(serial), $"Could not find {serial} in output.");
        serialAssertions("MKM/WC001-E095");
        serialAssertions("MKM/WC001-E100");
        serialAssertions("MKM/WC001-PE01");
    }
}
