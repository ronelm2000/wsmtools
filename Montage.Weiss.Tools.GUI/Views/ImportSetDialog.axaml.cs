using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommandLine;
using Montage.Weiss.Tools.GUI.ViewModels;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class ImportSetDialog : UserControl
{
    public ImportSetDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportSetViewModel vm)
            return;
        vm.IsVisible = false;
    }

    private void ParseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportSetViewModel viewModel)
            return;
        Task.Run(async () => await viewModel.ParseSet());
    }
}