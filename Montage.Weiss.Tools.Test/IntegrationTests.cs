using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Interfaces.Inputs;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test;

#nullable enable

[TestClass]
public class IntegrationTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Full Integration Test (Typical Use Case)")]
    [TestCategory("Manual")]
    public async Task FullTestRun()
    {
        await new ParseVerb()
        {
            URI = "https://heartofthecards.com/translations/love_live!_sunshine_school_idol_festival_6th_anniversary_booster_pack.html"
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        await new ParseVerb()
        {
            URI = "https://heartofthecards.com/translations/love_live!_sunshine_vol._2_booster_pack.html"
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        var testSerial = await Global.Container.GetInstance<CardDatabaseContext>().WeissSchwarzCards.FindAsync(["LSS/W69-006"], cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(testSerial is not null && testSerial.Images.Count != 0);

        var parseCommand = new ExportVerb()
        {
            Source = "https://www.encoredecks.com/deck/wDdTKywNh",
            NonInteractive = true
        };
        await parseCommand.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);
    }

    [TestMethod(DisplayName = "Exceptional Set Test (GFB vol. 2)")]
    [TestCategory("Manual")]
    public async Task GFBTestRun()
    {
        await new ParseVerb() { URI = "https://heartofthecards.com/translations/girl_friend_beta_booster_pack.html" }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);
        await new ParseVerb() { URI = "https://heartofthecards.com/translations/girl_friend_beta_vol.2_booster_pack.html" }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        var testSerial = await Global.Container.GetInstance<CardDatabaseContext>().WeissSchwarzCards.FindAsync(["GF/W38-020"], cancellationToken: TestContext.CancellationToken);
        Assert.AreNotEqual(0, testSerial?.Images.Count);
    }

    [TestMethod(DisplayName = "Help Test")]
    public async Task HelpTestRun()
    {
        Program.Console = Substitute.For<IConsole>();
        Program.Console.IsOutputRedirected.Returns(false);
        Program.Console.ReadKey(false).Returns(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        var task = Program.Main(["--help"]);
        await task;

        Program.Console.Received().ReadKey(false);
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod(DisplayName = "Regular Run (Update) Test")]
    public async Task RegularRunUpdateTest()
    {
        Program.Console = Substitute.For<IConsole>();
        Program.Console.IsOutputRedirected.Returns(false);
        Program.Console.ReadKey(false).Returns(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        var task = Program.Main(["update"]);
        await task;

        Program.Console.DidNotReceive().ReadKey(false);
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod(DisplayName = "EncoreDecks Parse + Yuyutei Update Test (S48)")]
    [TestCategory("Manual")]
    public async Task EncoreDecksParseAndYuyuteiUpdateTest()
    {
        // Step 1: Parse the EncoreDecks URL
        await new ParseVerb()
        {
            URI = "https://www.encoredecks.com/?cards=5e2c8834959adf655bdca150&page=1&set=5e2c87b1202372df5ea953bb"
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        // Verify S48 cards were parsed - ReleaseID is computed, so materialize first
        var db = Global.Container.GetInstance<CardDatabaseContext>();
        var s48Cards = db.WeissSchwarzCards.ToList().Where(c => c.ReleaseID == "S48").ToList();
        Assert.IsTrue(s48Cards.Count > 0, "S48 cards should have been parsed from EncoreDecks");

        // Step 2: Run update with Yuyutei post-processor
        await new UpdateVerb()
        {
            ReleaseIDs = "S48",
            PostProcessorAliases = "yyt"
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        // Verify update completed by checking that cards still exist
        var updatedCards = db.WeissSchwarzCards.ToList().Where(c => c.ReleaseID == "S48").ToList();
        Assert.IsTrue(updatedCards.Count > 0, "S48 cards should still exist after update");
    }
}
