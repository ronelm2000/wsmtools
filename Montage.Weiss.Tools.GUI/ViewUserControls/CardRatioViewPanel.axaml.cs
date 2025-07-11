using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Montage.Weiss.Tools.GUI.ViewModels;
using Serilog;
using System;

namespace Montage.Weiss.Tools.GUI.ViewUserControls;

public partial class CardRatioViewPanel : UserControl
{
    public static Func<ILogger?> Logger { get; set; } = () => Serilog.Log.ForContext<CardRatioViewPanel>();
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<ModifyTranslationEventArgs> RequestedTranslationsEvent = RoutedEvent.Register<Button, ModifyTranslationEventArgs>(nameof(RequestTranslations), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the user clicks the button.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>
    /// Raised when the user requests to change thr translation of a card.
    /// </summary>
    public event EventHandler<ModifyTranslationEventArgs>? RequestTranslations
    {
        add => AddHandler(RequestedTranslationsEvent, value);
        remove => RemoveHandler(RequestedTranslationsEvent, value);
    }

    public CardRatioViewPanel()
    {
        InitializeComponent();
    }

    public void Generic_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            Logger()?.Debug("Generic_PointerReleased: Not a left click, ignoring.");
            return;
        }
        var args = new RoutedEventArgs(ClickEvent, this);
        Logger()?.Information("Raised (Generic_PointerReleased): {args}", args.ToString());
        RaiseEvent(args);
    }

    public void ModifyTranslation_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            Logger()?.Debug("ModifyTranslation_PointerReleased: Not a left click, ignoring.");
            return;
        }
        var args = new ModifyTranslationEventArgs(RequestedTranslationsEvent, (DataContext as CardRatioViewModel)!, this);
        Logger()?.Information("Raised (ModifyTranslation_PointerReleased): {args}", args.ToString());
        RaiseEvent(args);
    }
}

public class ModifyTranslationEventArgs : RoutedEventArgs
{
    public ModifyTranslationEventArgs(RoutedEvent routedEvent, CardRatioViewModel context, object? source = null) : base(routedEvent, source)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public CardRatioViewModel Context { get; init; }
}