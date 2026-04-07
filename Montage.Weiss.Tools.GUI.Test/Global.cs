using Lamar;

namespace Montage.Weiss.Tools.GUI.Test;

[SetUpFixture]
public class Global
{
    public static IContainer Container { get; private set; }


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Container = App.InitializeIoC();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Container.Dispose();
    }
}
