using Avalonia.Controls;
using Avalonia.Threading;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class ImportDeckDialog : UserControl
{
    private IDisposable? currentlyPendingTask;

    public ImportDeckDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportDeckViewModel vm)
            return;
        vm.IsVisible = false;
        currentlyPendingTask?.Dispose();
    }

    private async void ParseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportDeckViewModel vm)
            return;

        currentlyPendingTask = Observable.FromAsync(ct => vm.ImportDeck(ct))
            .SubscribeOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(c => { }
            , ex =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    vm.Parent()!.Status = $"Error: {ex.Message}";
                });
            });
    }
}