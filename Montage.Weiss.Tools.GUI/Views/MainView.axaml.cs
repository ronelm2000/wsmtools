using AngleSharp.Dom;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using Montage.Weiss.Tools.GUI.ViewModels;
using Montage.Weiss.Tools.GUI.ViewUserControls;
using Montage.Weiss.Tools.Impls.Services;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class MainView : UserControl {
    public static Func<ILogger?> Logger { get; set; } = () => Serilog.Log.ForContext<DatabaseCardViewPanel>();

    /*
    public MainView(MainViewModel mainViewModel)
    {
        DataContext = mainViewModel;
        InitializeComponent();
    }
    */

    /// <summary>
    /// Note: Only used with the previewer
    /// </summary>
    public MainView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        if (viewModel.Container is null)
            return;

        Task.Run(viewModel.Load);
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
}
