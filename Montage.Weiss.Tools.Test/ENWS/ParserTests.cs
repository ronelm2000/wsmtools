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

namespace Montage.Weiss.Tools.Test.ENWS
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod("EN WS Parser Test")]
        [Ignore("Currently not working due to a change in site. WIP")]
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
            list = await new EnglishWSURLParser().Parse(url, progressReporter, CancellationToken.None).ToListAsync();
            Log.Information("Cards Obtained: {length}", list.Count);

            foreach (var card in list)
                Log.Information("Card: {@card}", card.Serial);
        }
    }
}
