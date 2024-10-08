using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Lamar;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.GUI.Utilities;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
public partial class ImportDeckViewModel : ViewModelBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ImportDeckViewModel>();

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _isCommandEnabled;

    [ObservableProperty]
    private Func<MainWindowViewModel?> _parent;

    [ObservableProperty]
    private string deckUrl;

    public ImportDeckViewModel()
    {
        Parent = () => null;
        IsVisible = Design.IsDesignMode;
        IsCommandEnabled = true;
        DeckUrl = string.Empty;
    }

    internal async Task ImportDeck()
    {
        if (Parent() is not MainWindowViewModel parentModel)
            return;
        if (parentModel.Container is not IContainer container)
            return;

        IsCommandEnabled = false;

        try
        {
            var progressReporter = new ProgressReporter(Log, message => parentModel.Status = message);
            var command = new ExportVerb { Source = DeckUrl, NonInteractive = true, NoWarning = true };
            var deck = await command.Parse(container, progressReporter);

            parentModel.DeckName = deck.Name;
            parentModel.DeckRemarks = deck.Remarks;

            var cacheList = deck.Ratios.Keys.Where(c => c.GetCachedImagePath() is null && c.EnglishSetType != EnglishSetType.Custom).ToAsyncEnumerable();
            await new CacheVerb { }.Cache(container, progressReporter, cacheList);

            parentModel.DeckRatioList.Clear();

            foreach (var ratio in deck.Ratios)
                parentModel.DeckRatioList.Add(new CardRatioViewModel(ratio.Key, ratio.Value));

            parentModel.SortDeck();
            parentModel.UpdateDeckStats();

            parentModel.Status = $"Done Importing: {DeckUrl}";

        }
        catch (Exception ex)
        {
            Log.Error("Error occurred", ex);
            parentModel.Status = ex.Message;
        } finally
        {

            IsVisible = false;
            IsCommandEnabled = true;
        }
    }
}
