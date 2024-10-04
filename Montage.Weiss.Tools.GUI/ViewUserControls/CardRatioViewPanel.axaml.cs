using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Serilog;
using System;

namespace Montage.Weiss.Tools.GUI.ViewUserControls;

public partial class CardRatioViewPanel : UserControl
{
    public static Func<ILogger?> Logger { get; set; } = () => Serilog.Log.ForContext<CardRatioViewPanel>();
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the user clicks the button.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public CardRatioViewPanel()
    {
        InitializeComponent();
    }

    public void Generic_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var args = new RoutedEventArgs(ClickEvent, this);
        Logger()?.Information("Raised (Generic_PointerReleased): {args}", args.ToString());
        RaiseEvent(args);
    }
}