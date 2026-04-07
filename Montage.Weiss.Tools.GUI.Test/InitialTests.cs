using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.VisualTree;
using Montage.Weiss.Tools.GUI.ViewModels;
using Montage.Weiss.Tools.GUI.Views;

namespace Montage.Weiss.Tools.GUI.Test;

public class InitialTests

{
    [AvaloniaTest]
    [TestCase(TestName = "MainMenu Exists")]
    public void TestMainMenuShouldExist()
    {
        var window = Global.Container.GetInstance<MainWindow>();

        window.Show();

        var mainView = window.FindDescendantOfType<MainView>();
        var mainMenu = mainView!.Find<Menu>("MainMenu");

        Assert.IsNotNull(mainMenu);
    }

    [AvaloniaTest]
    [TestCase(TestName = "DatabaseView Toggles")]
    public void TestDatabaseViewShouldToggle()
    {
        var window = Global.Container.GetInstance<MainWindow>();

        window.Show();

        var mainView = window.FindDescendantOfType<MainView>();
        var dataContext = window.DataContext as MainWindowViewModel;

        dataContext!.ToggleDatabaseViewCommand.Execute(null);

        var databaseView = mainView!.Find<Border>("DatabaseView");

        Assert.IsTrue(databaseView!.IsVisible);
    }
}
