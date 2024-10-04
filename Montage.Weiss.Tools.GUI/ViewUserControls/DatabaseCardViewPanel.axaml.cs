using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Serilog;
using System;

namespace Montage.Weiss.Tools.GUI.ViewUserControls;

public partial class DatabaseCardViewPanel : UserControl
{
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<DatabaseCardViewPanel, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble | RoutingStrategies.Direct);
    public static Func<ILogger?> Logger { get; set; } = () => Serilog.Log.ForContext<DatabaseCardViewPanel>();

    public static void AddClickHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
         element.AddHandler(ClickEvent, handler);

    public static void RemoveClickHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
         element.RemoveHandler(ClickEvent, handler);

    /// <summary>
    /// Raised when the user clicks the button.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public DatabaseCardViewPanel()
    {
        InitializeComponent();
    }

    private void Generic_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        var args = new RoutedEventArgs(ClickEvent, this);
        Logger()?.Information("Raised (Generic_PointerReleased): {args}", args.ToString());
        RaiseEvent(args);
    }
}