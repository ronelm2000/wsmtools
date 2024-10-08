using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class ImportDeckDialog : UserControl
{
    public ImportDeckDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportDeckViewModel vm)
            return;
        vm.IsVisible = false;
    }

    private void ParseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportDeckViewModel vm)
            return;
        Task.Run(async () => await vm.ImportDeck());
    }
}