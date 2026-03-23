using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Interfaces.Inputs;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Test.Commons;
using NSubstitute;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test;

#nullable enable

[TestClass]
public class IntegrationTests
{
    [TestMethod(DisplayName = "Full Integration Test (Typical Use Case)")]
    [TestCategory("Manual")]
    public async Task FullTestRun()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progress = NoOpProgress<CommandProgressReport>.Instance;

        await new ParseVerb()
        {
            URI = "https://heartofthecards.com/translations/love_live!_sunshine_school_idol_festival_6th_anniversary_booster_pack.html"
        }.Run(ioc, progress);

        await new ParseVerb()
        {
            URI = "https://heartofthecards.com/translations/love_live!_sunshine_vol._2_booster_pack.html"
        }.Run(ioc, progress);

        var testSerial = await ioc.GetInstance<CardDatabaseContext>().WeissSchwarzCards.FindAsync("LSS/W69-006");
        Assert.IsTrue(testSerial is not null && testSerial.Images.Any());

        var parseCommand = new ExportVerb()
        {
            Source = "https://www.encoredecks.com/deck/wDdTKywNh",
            NonInteractive = true
        };
        await parseCommand.Run(ioc, progress);
    }

    [TestMethod(DisplayName = "Exceptional Set Test (GFB vol. 2)")]
    [TestCategory("Manual")]
    public async Task GFBTestRun()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        var ioc = Program.Bootstrap();
        var progressReporter = NoOpProgress<object>.Instance;

        await new ParseVerb() { URI = "https://heartofthecards.com/translations/girl_friend_beta_booster_pack.html" }.Run(ioc, progressReporter);
        await new ParseVerb() { URI = "https://heartofthecards.com/translations/girl_friend_beta_vol.2_booster_pack.html" }.Run(ioc, progressReporter);

        var testSerial = await ioc.GetInstance<CardDatabaseContext>().WeissSchwarzCards.FindAsync("GF/W38-020");
        Assert.IsTrue(testSerial?.Images.Any());
    }

    [TestMethod(DisplayName = "Help Test")]
    public async Task HelpTestRun()
    {
        Program.Console = Substitute.For<IConsole>();
        Program.Console.IsOutputRedirected.Returns(false);
        Program.Console.ReadKey(false).Returns(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        var task = Program.Main(new[] { "--help" });
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

        var task = Program.Main(new[] { "update" });
        await task;

        Program.Console.DidNotReceive().ReadKey(false);
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }
}
