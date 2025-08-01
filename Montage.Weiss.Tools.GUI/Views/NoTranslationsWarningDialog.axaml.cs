using Avalonia.Controls;
using CommandLine;
using Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Views;

public partial class NoTranslationsWarningDialog : UserControl
{
    public NoTranslationsWarningDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not NoTranslationsWarningViewModel viewModel)
            return;
        viewModel.CancelDialog();
    }

    private void ContinueButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not NoTranslationsWarningViewModel viewModel)
            return;
        viewModel.AcceptDialog();
    }
}