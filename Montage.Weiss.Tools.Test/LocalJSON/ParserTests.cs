using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.LocalJSON;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Full Integration Test (Local JSON) (Typical Use Case)")]
    [DeploymentItem("Resources/deck_date_a_live.json")]
    [Timeout(60000)] // 1 min 
    public async Task FullTestRun()
    {
        await new ParseVerb()
        {
            URI = "https://www.encoredecks.com/?page=1&set=5cfbffe67cd9b718cdf4b439"
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        await new ExportVerb()
        {
            Source = "./deck_date_a_live.json",
            Exporter = "local",
            NonInteractive = true,  
            NoWarning = true
        }.Run(Global.Container, Global.MockProgress, TestContext.CancellationToken);

        Assert.IsTrue(Path.Get("./Export/deck_date_a_live.ws-dek").Exists);
    }
}
