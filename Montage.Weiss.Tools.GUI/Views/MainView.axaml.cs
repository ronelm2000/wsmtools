using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Montage.Weiss.Tools.GUI.ViewModels;
using Montage.Weiss.Tools.GUI.ViewUserControls;
using Serilog;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class MainView : UserControl
{
    private static ILogger Log { get; } = Serilog.Log.ForContext<DatabaseCardViewPanel>();


    /// <summary>
    /// Note: Only used with the previewer
    /// </summary>
    public MainView()
    {
        InitializeComponent();
    }

    private void DeckView_TouchPadSwiped(object? sender, PointerDeltaEventArgs e)
    {
        var a = e.Delta;
        var timeStamp = e.Timestamp;

        Log.Information("Delta: {d} / Timestamp: {e}", a, timeStamp);
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (viewModel.Container is null)
            return;

        Observable.FromAsync(viewModel.Load).Subscribe(_ => { }, ex =>
        {
            Log.Error(ex, "Error Loading the App.");
        });
    }

    private void DatabaseCardViewPanel_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (sender is not DatabaseCardViewPanel viewPanel)
            return;
        if (viewPanel.DataContext is not CardEntryViewModel cardEntryViewModel)
            return;

        Task.Run(async () => await viewModel.AddCard(cardEntryViewModel.Card));
    }

    private void CardRatioViewPanel_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (sender is not CardRatioViewPanel viewPanel)
            return;
        if (viewPanel.DataContext is not CardRatioViewModel cardRatioViewModel)
            return;

        Task.Run(async () => await viewModel.RemoveCard(cardRatioViewModel.Card));
    }

    private void CardRatioViewPanel_RequestTranslations(object? sender, ModifyTranslationEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (e.Source is not CardRatioViewPanel viewPanel)
            return;
        if (viewPanel.DataContext is not CardRatioViewModel cardRatioViewModel)
            return;
        Task.Run(async () => await viewModel.OpenModifyTranslations(cardRatioViewModel));
    }

    private void ScrollViewer_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        var delta = e.Delta * scrollViewer.SmallChange.Height * -1;

        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
            delta *= 30; //.WithY(delta.Y * 4);

        scrollViewer.Offset += delta;
    }

    private void SearchQueryBorder_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (e.Source is not IDataContextProvider dcp)
            return;
        if (dcp.DataContext is not CardSearchQueryViewModel cardSearchQueryViewModel)
            return;
        viewModel.SearchQueries.Remove(cardSearchQueryViewModel);
    }

    private void DeckView_PullGesture(object? sender, PullGestureEventArgs e)
    {
        Log.Information("Swiped");
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.IsDatabaseOpen = true;
        e.Handled = true;
    }

    internal void WindowOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            viewModel.IsShiftPressed = !viewModel.IsShiftPressed;
            Log.Verbose("KeyDown: {key} (Modifiers: {modifiers}) | IsShiftPressed: {status}", e.Key, e.KeyModifiers, viewModel.IsShiftPressed);

        }
    }
}
