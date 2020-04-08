using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.ENWS
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod("EN WS Parser Test")]
        public async Task TestParser()
        {
            Serilog.Log.Logger = IntegrationTests.BootstrapLogging().CreateLogger();
            var stream = new EnglishWSURLParser().Parse("https://en.ws-tcg.com/cardlist/list/?cardno=CCS/BSF2019-02");
            await foreach (var card in stream)
                Log.Information("Card: {@card}", card);

            stream = new EnglishWSURLParser().Parse("https://en.ws-tcg.com/cardlist/list/?cardno=FS/S36-E018");
            await foreach (var card in stream)
                Log.Information("Card: {@card}", card);
        }

    }
}
