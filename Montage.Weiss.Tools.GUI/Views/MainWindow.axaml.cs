using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Serilog;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class MainWindow : Window
{
    private static ILogger Log = Serilog.Log.ForContext<MainWindow>();
    /// <summary>
    /// Note: Only used with the previewer
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("MainWindow loaded. Adding global key down handler for the main view.");
        var mainView = this.FindDescendantOfType<MainView>();
        this.AddHandler(InputElement.KeyDownEvent, mainView!.WindowOnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
    }
}
