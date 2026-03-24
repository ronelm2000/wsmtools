using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Test.Commons;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.EncoreDecks;

[TestClass]
public class ServiceTests
{
    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "Set List Test")]
    public async Task TestSetListTestAsync()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        
        var ioc = Program.Bootstrap();
        var edSrvc = ioc.GetInstance<EncoreDecksService>();
        
        var allSets = await edSrvc.GetSetListEntries(TestContext.CancellationToken);
        
        Assert.AreEqual("Accel World", allSets[0].Name);
    }
}
