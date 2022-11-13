using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.EncoreDecks;

[TestClass]
public class ServiceTests
{
    [TestMethod("Set List Test")]
    public async Task TestSetListTestAsync()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Lamar.Container ioc = Program.Bootstrap();
        var edSrvc = ioc.GetInstance<EncoreDecksService>();
        var cts = new CancellationTokenSource();
        var allSets = await edSrvc.GetSetListEntries(cts.Token);
        Assert.IsTrue(allSets[0].Name == "Accel World");
    }
}
