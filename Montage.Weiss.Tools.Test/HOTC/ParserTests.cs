﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.HOTC
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod("Full Integration Test (Local Text File) (HOTC) (Typical Use Case)")]
        [DeploymentItem("Resources/shiyoko_prs_hotc.txt")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();
            var progress = NoOpProgress<CommandProgressReport>.Instance;
            await new ParseVerb()
            {
                URI = "./shiyoko_prs_hotc.txt",
                ParserHints = new string[] { "hotc" }
            }.Run(ioc, progress);
        }

        [TestMethod("HOTC Parser Trait Test")]
        public async Task TestTraitHandling()
        {
            var progress = NoOpProgress<SetParserProgressReport>.Instance;
            var lba = await new HeartOfTheCardsURLParser().Parse("https://www.heartofthecards.com/translations/little_busters!_anime_booster_pack.html", progress, CancellationToken.None) //
                .ToDictionaryAsync(c => c.Serial);

            Assert.IsTrue(lba["LB/W21-046"].Traits.Count == 1, $"LB/W21-046 has an invalid amount of traits: {lba["LB/W21-046"].Traits.Count}");
            Assert.IsTrue(lba["LB/W21-065"].Traits.Count == 0, $"LB/W21-065has an invalid amount of traits: {lba["LB/W21-065"].Traits.Count}");
        }
    }
}
