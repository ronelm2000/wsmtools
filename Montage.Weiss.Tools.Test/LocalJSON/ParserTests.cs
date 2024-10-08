using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.LocalJSON;

[TestClass]
public class ParserTests
{
    [TestMethod("Full Integration Test (Local JSON) (Typical Use Case)")]
    [DeploymentItem("Resources/deck_date_a_live.json")]
    public async Task FullTestRun()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var progressReporter = NoOpProgress<CommandProgressReport>.Instance;

        await new ParseVerb()
        {
            URI = "https://www.encoredecks.com/?page=1&set=5cfbffe67cd9b718cdf4b439"
        }.Run(ioc, progressReporter);

        await new ExportVerb()
        {
            Source = "./deck_date_a_live.json",
            Exporter = "local",
            NonInteractive = true,
            NoWarning = true
        }.Run(ioc, progressReporter);

        Assert.IsTrue(Path.Get("./Export/deck_date_a_live.ws-dek").Exists);
    }
}
