using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ImTools;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.EncoreDeck;
using Montage.Weiss.Tools.Impls.Parsers.Cards;
using Montage.Weiss.Tools.Utilities;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
public partial class ImportTranslationsViewModel : ViewModelBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ImportTranslationsViewModel>();

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _isCommandEnabled;

    [ObservableProperty]
    private Func<MainWindowViewModel?> _parent;

    [ObservableProperty]
    private string _customEnglishName;

    [ObservableProperty]
    private string _customTranslation;

    [ObservableProperty]
    private string _insertTranslationsText;

    private WeissSchwarzCard _targetCard;

    public ImportTranslationsViewModel()
    {
        Parent = () => null;
        IsVisible = Design.IsDesignMode;
        IsCommandEnabled = true;
        CustomTranslation = string.Empty;
        CustomEnglishName = string.Empty;
        InsertTranslationsText = "Insert Translations For {Card}...";
        _targetCard = WeissSchwarzCard.Empty;
    }

    public async Task ApplyTarget(WeissSchwarzCard card)
    {
        _targetCard = card;
        
        InsertTranslationsText = string.Format("Insert Translations For {0} [{1}]", card.Name.AsNonEmptyString(), card.Serial);
        CustomTranslation = (card.Effect ?? []).ConcatAsString(Environment.NewLine);
        CustomEnglishName = card.Name.EN ?? string.Empty;

        await ValueTask.CompletedTask;
    }

    internal async Task ApplyTranslationsOnTarget()
    {
        if (Parent() is not MainWindowViewModel parentModel)
            return;
        if (parentModel.Container is not IContainer container)
            return;

        Log.Information("Applying translations on target card: {Card}", _targetCard.Serial);

        using var database = container.GetInstance<CardDatabaseContext>();

        await database.Database.MigrateAsync();

        var card = await database.WeissSchwarzCards
            .FirstOrDefaultAsync(c => c.Serial == _targetCard.Serial);

        if (card is not null) {
            card.Name = card.Name with { EN = CustomEnglishName };
            card.Effect = CustomTranslation.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            Log.Information("Applied translations: {Card}", _targetCard.Serial);

            var optionalInfoForED = card.FindOptionalInfo<EncoreDeckOptionalInfo>(EncoreDecksParser.ParserInfoKey);
            if (optionalInfoForED is not null)
            {
                optionalInfoForED = optionalInfoForED with { HasEnglishTranslations = true };
                card.AddOptionalInfo(EncoreDecksParser.ParserInfoKey, optionalInfoForED);
                Log.Information("Updated EncoreDecks Optional Info for {Card}", _targetCard.Serial);
            }

            var ratioModel = parentModel.DeckRatioList.First(vm => vm.Card.Serial == _targetCard.Serial)!;
            ratioModel.Card = _targetCard;
            ratioModel.Card.Name = card.Name;
            ratioModel.Card.Effect = card.Effect;
            ratioModel.Effects = [.. card.Effect];

            Log.Information("Applied Translations on local copy as well.");
        }
        else
        {
            Log.Warning("Card not found in database: {Card}", _targetCard.Serial);
            parentModel.Status = $"Card not found: {_targetCard.Serial}";
        }

        await database.SaveChangesAsync();

        IsVisible = false;
    }
}
