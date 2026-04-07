using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Montage.Weiss.Tools.GUI.ViewModels;
using Serilog;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class MainWindow : Window
{
    private static ILogger Log = Serilog.Log.ForContext<MainWindow>();

    public MainWindow(MainWindowViewModel model)
    {
        DataContext = model;
        InitializeComponent();
    }

    /// <summary>
    /// Note: Only used with the previewer
    /// </summary>
    public MainWindow()
    {
        DataContext = new MainWindowViewModel
        {
            Parent = () => this,
            Container = App.InitializeIoC()
        };
        InitializeComponent();
    }

    private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("MainWindow loaded. Adding global key down handler for the main view.");
        var mainView = this.FindDescendantOfType<MainView>();
        this.AddHandler(InputElement.KeyDownEvent, mainView!.WindowOnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
    }
}
