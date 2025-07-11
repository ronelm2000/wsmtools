using Avalonia.Controls;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class ImportCustomTranslationsDialog : UserControl
{
    public ImportCustomTranslationsDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportTranslationsViewModel vm)
            return;
        vm.IsVisible = false;
    }

    private void ApplyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ImportTranslationsViewModel viewModel)
            return;
        Task.Run(async () => await viewModel.ApplyTranslationsOnTarget());
    }
}