using Lamar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Test.Commons;

namespace Montage.Weiss.Tools.Test;

[TestClass]
public static class Global
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static IContainer Container { get; private set; }
    public static NoOpProgress<object> MockProgress { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [AssemblyInitialize]
    public static void GlobalTestInit(TestContext context)
    {
        // Runs before every test method in the assembly
        context.WriteLine($"Starting Tests... Initializing logging.");

        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
        Container = Program.Bootstrap();
        MockProgress = NoOpProgress<object>.Instance;
    }

    [AssemblyCleanup]
    public static void GlobalTestCleanup(TestContext context)
    {
        // Runs after every test method in the assembly
        context.WriteLine("Ending all tests...");
    }
}
