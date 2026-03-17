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

    public static Func<ILogger> Logger { get; set; } = () => Serilog.Log.ForContext<DatabaseCardViewPanel>();

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

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            Logger().Information("Shift key state changed: {key} (Modifiers: {modifiers})", e.Key, e.KeyModifiers);
        }
    }

    private void Generic_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        var args = new RoutedEventArgs(ClickEvent, this);
        Logger()?.Information("Raised (Generic_PointerReleased): {args}", args.ToString());
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