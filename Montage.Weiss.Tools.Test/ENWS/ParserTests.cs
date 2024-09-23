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

namespace Montage.Weiss.Tools.Test.ENWS;

[TestClass]
public class ParserTests
{
    [TestMethod("EN WS Parser Test")]
    [Ignore("Currently not working due to new site.")]
    public async Task TestParser()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var progressReporter = NoOpProgress<object>.Instance;

        var url = "https://en.ws-tcg.com/cardlist/list/?cardno=CCS/BSF2019-02";
        var list = await new EnglishWSURLParser().Parse(url, progressReporter, CancellationToken.None).ToListAsync();
        Log.Information("Cards Obtained: {length}", list.Count);
        foreach (var card in list)
            Log.Information("Card: {@card}", card.Serial);

        url = "https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E018";
        var dict = await new EnglishWSURLParser().Parse(url, progressReporter, CancellationToken.None)
            .ToDictionaryAsync(c => c.Serial, c => c);
        Log.Information("Cards Obtained: {length}", dict.Keys.Count);

        Action<string> serialAssertions = serial => Assert.IsTrue(dict.ContainsKey(serial), $"Could not find {serial} in output.");
        serialAssertions("FS/S36-PE02");
        serialAssertions("FS/S36-E012");
        serialAssertions("FS/S36-PE01");
    }
}
