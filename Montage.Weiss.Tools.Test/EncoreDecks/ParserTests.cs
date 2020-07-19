using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Test.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.EncoreDecks
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod("Batman Ninja / BDML / LB Anime Full Parse Test")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            var batmanNinja = new Tools.Impls.Parsers.Cards.EncoreDecksParser().Parse("https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards");
            Assert.IsTrue(await batmanNinja.AnyAsync(c => c.Serial == "BNJ/BCS2019-02"));

            var bdml = new Tools.Impls.Parsers.Cards.EncoreDecksParser().Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469");
            Assert.IsTrue(await bdml.AnyAsync(c => c.Serial == "BD/EN-W03-007" || c.Effect.Length < 1), "BD/EN-W03 might have not been parsed correctly; do check.");

            var lbAnime = new Tools.Impls.Parsers.Cards.EncoreDecksParser().Parse("https://www.encoredecks.com/?page=1&set=5d6d4e147cd9b718cd3a0d40");
            Log.Information("LB Anime Set has been parsed successfully. This was a set that is not translated when this Unit Test was created.");
            var serialChecklist = new[] { "LB/W21-078", "LB/W21-065" };
            Assert.IsTrue(await bdml.AnyAsync(c => !serialChecklist.Contains(c.Serial)), "LB/W21 might have not been parsed correctly; do check.");
        }

        [TestMethod("Batman Ninja Trait Test")]
        public async Task TraitTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();
            var batmanNinja = await new Tools.Impls.Parsers.Cards.EncoreDecksParser().Parse("https://www.encoredecks.com/api/series/5d3232ec7cd9b718cd126e2e/cards") //
                .ToDictionaryAsync(c => c.Serial);
            Assert.IsTrue(batmanNinja["BNJ/SX01-T07"].Traits.Count == 0);
            Assert.IsTrue(batmanNinja["BNJ/SX01-A13"].Traits.Count == 0);
            Assert.IsTrue(batmanNinja["BNJ/SX01-078b"].Traits.Select(mls => mls.EN).All(trait => new[] { "Sengoku", "Weapon" }.Contains(trait)));
        }

        [TestMethod("BDML Climax Trigger Test")]
        public async Task TestClimaxTriggers()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();
            var bdml = await new Tools.Impls.Parsers.Cards.EncoreDecksParser().Parse("https://www.encoredecks.com/?page=1&set=5cf701347cd9b718cdf21469")
                .ToDictionaryAsync(c => c.Serial);
            Assert.IsTrue(bdml["BD/EN-W03-125"].Triggers.Length == 2);
            Assert.IsTrue(bdml["BD/EN-W03-125"].Triggers.Contains(Trigger.Gate));
            Assert.IsTrue(bdml["BD/EN-W03-126"].Triggers.Contains(Trigger.Book));

        }
    }
}
