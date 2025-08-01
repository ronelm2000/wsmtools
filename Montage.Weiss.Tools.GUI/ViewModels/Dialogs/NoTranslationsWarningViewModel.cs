using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DynamicData;
using Montage.Weiss.Tools.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
public partial class NoTranslationsWarningViewModel: ViewModelBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<NoTranslationsWarningViewModel>();

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private Func<MainWindowViewModel?> _parent;

    [ObservableProperty]
    private string _WarningText;
    public ObservableCollection<CardEntryViewModel> TargetCards { get; } = new();

    private TaskCompletionSource<bool> dialogCompletionSource;

    public NoTranslationsWarningViewModel()
    {
        WarningText = "This card is considered to not have any translations available. " +
            "In certain cases, this may be due to English text not available, " +
            "or the card not being considered as translated by the original parser itself (typically EncoreDecks using LittleAkiba translations). " +
            "Do you wish to continue? Or modify the translations first via UI?";
        Parent = () => null;
        IsVisible = Design.IsDesignMode;
        dialogCompletionSource = new TaskCompletionSource<bool>(false);
        if (Design.IsDesignMode)
        {
            TargetCards.Add(CardEntryViewModel.Sample);
        }
    }

    internal void CancelDialog()
    {
        TargetCards.Clear();
        IsVisible = false;
        dialogCompletionSource.SetResult(false);
    }

    internal void AcceptDialog()
    {
        TargetCards.Clear();
        IsVisible = false;
        dialogCompletionSource.SetResult(true);
    }

    public async Task<bool> ShowDialogAsync(IEnumerable<WeissSchwarzCard> cardsWithNoTranslation)
    {
        try
        {
            TargetCards.AddRange(cardsWithNoTranslation.Select(c => new CardEntryViewModel(c)));
            if (TargetCards.Count < 2)
            {
                WarningText = "This card is considered to not have any translations available.";
            }
            else
            {
                WarningText = "These cards are considered to not have any translations available.";
            }
            WarningText += " In certain cases, this may be due to English text not available, " +
                "or the card not being considered as translated by the original parser itself (typically EncoreDecks using LittleAkiba translations). " +
                "Do you wish to continue? Or modify the translations first via UI?";
            IsVisible = true;
            var result = await dialogCompletionSource.Task;
            return result;
        }
        finally
        {
            TargetCards.Clear();
            dialogCompletionSource = new TaskCompletionSource<bool>(false);
        }
    }
}
