using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.GUI.Views;
using Serilog;
using System;
using System.Linq;

namespace Montage.Weiss.Tools.GUI.ViewUserControls;

public partial class DatabaseCardViewPanel : UserControl
{
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<DatabaseCardViewPanel, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble | RoutingStrategies.Direct);

    private static ILogger Log = Serilog.Log.ForContext<DatabaseCardViewPanel>();

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
        if (Log.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            Log.Verbose("Raised (Generic_PointerReleased): {@args}", args);
        RaiseEvent(args);
    }

    private void UserControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        var parents = this.GetLogicalAncestors().OfType<MainWindow>().FirstOrEmpty();
        if (!this.IsFocused && !(parents?.IsFocused ?? false))
        {
            // I'll re-add this probably later as the idea of auto-focusing the control to the search textbox might be a good idea.
            // Need more user feedback on this though as it could be annoying if the user just wanted to hover over the card and steal focus from the deck name or notes.

            //Logger()?.Debug("Focusing unfocused control so that Shift works.");
            //var searchTB = parents.FindLogicalDescendantOfType<TextBox>();
            //searchTB?.Focus();
        }
    }
}