using CommunityToolkit.Mvvm.ComponentModel;
using Montage.Card.API.Entities.Impls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Montage.Weiss.Tools.GUI.Extensions;
using Montage.Weiss.Tools.Entities;
using System.Linq;
using Montage.Weiss.Tools.Utilities;
using DynamicData.Binding;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;
using Avalonia.Platform;
using Serilog;
using System.Reactive;
using Montage.Weiss.Tools.GUI.ViewModels.Query;
using Avalonia.Threading;
using System.Diagnostics.CodeAnalysis;
using Montage.Weiss.Tools.CLI;
using Montage.Card.API.Services;


namespace Montage.Weiss.Tools.GUI.ViewModels;

public partial class CardEntryViewModel : ViewModelBase
{
    private static ILogger Log = Serilog.Log.ForContext<CardEntryViewModel>();
    public static Bitmap LoadingImage { get; } = new Bitmap(AssetLoader.Open(new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg")));
    public static CardEntryViewModel Sample { get; } = new(
        new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg"),
        new MultiLanguageString { EN = "Sample 1", JP = "Sample 1 But JP" },
        [
                new MultiLanguageString { EN = "AAAA", JP = "AAAA JP" },
                new MultiLanguageString { EN = "BBBB", JP = "BBBB JP" }
        ]
        );

    [ObservableProperty]
    private WeissSchwarzCard _card;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _serial;

    [ObservableProperty]
    private List<MultiLanguageString> _traits;

    [ObservableProperty]
    private Trigger _trigger;

    [ObservableProperty]
    private int? _level;

    [ObservableProperty]
    private int? _cost;

    [ObservableProperty]
    private CardType _cardType;

    [ObservableProperty]
    private int? _power;

    [ObservableProperty]
    private int? _soul;

    [ObservableProperty]
    private List<Bitmap> _effectMarkers;

    [ObservableProperty]
    private List<string> _effects;

    public MainWindowViewModel? Parent { get; init; } = null;

    public IObservable<bool> IsCharacter { get; private set; }
    public IObservable<bool> IsCharacterOrEvent { get; private set; }

    public ReactiveCommand<Unit, Unit> FindClimaxCombosCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> FindTraitsCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> FindNamesCommand { get; private set; }

    public Task<Bitmap?> Image => FindImageAsync();

    private Task<Bitmap?>? _cachedImageTask;

    public CardEntryViewModel(Uri imageUri, MultiLanguageString name, List<MultiLanguageString> traits)
    {
        DeclareObservables();

        Card = new WeissSchwarzCard();
        Name = name.AsNonEmptyString();
        Traits = traits;
        Serial = "XXX/WS01-100";
        Level = 1;
        Cost = 0;
        Power = 5000;
        Soul = 1;
        CardType = CardType.Character;
        EffectMarkers = new() { Bitmap.DecodeToHeight(AssetLoader.Open(new Uri("avares://wsm-gui/Assets/Symbols/cx_combo.png")), 12) };
        Effects = ["[AUTO] Aaaaaaaaa"];

        _cachedImageTask = imageUri.Load();
    }

    public CardEntryViewModel(WeissSchwarzCard card, MainWindowViewModel parent)
    {
        DeclareObservables();

        Card = card;
        Name = card.Name.AsNonEmptyString();
        Traits = card.Traits.Select(t => new MultiLanguageString {  EN = t.EN, JP = t.JP }).ToList();
        Serial = card.Serial;
        CardType = card.Type;
        Level = card.Level;
        Cost = card.Cost;
        Power = card.Power;
        Soul = card.Soul;
        EffectMarkers = card.Effect.SelectMany(e => TranslateEffect(e))
            .Distinct()
            .Select(u => Bitmap.DecodeToHeight(AssetLoader.Open(u), 12))
            .ToList();
        Parent = parent;
        Effects = card.Effect.ToList();
    }

    private async Task<Bitmap?> FindImageAsync()
    {
        if (_cachedImageTask is not null)
            return await _cachedImageTask;
        else
            return await (_cachedImageTask = LoadCardAsync(Card));
    }

    private async Task<Avalonia.Media.Imaging.Bitmap?> LoadCardAsync(WeissSchwarzCard card)
    {
        var ioc = Parent!.Container!;
        var cachedImagePath = card.GetCachedImagePath();
        if (cachedImagePath is null && card.EnglishSetType != EnglishSetType.Custom)
            await new CacheVerb { }.Cache(ioc, NoOpProgress<CommandProgressReport>.Instance, card);

        return await card.LoadImage();
    }

    [MemberNotNull(
        nameof(IsCharacter), 
        nameof(IsCharacterOrEvent), 
        nameof(FindClimaxCombosCommand), 
        nameof(FindClimaxCombosCommand), 
        nameof(FindTraitsCommand),
        nameof(FindNamesCommand)
        )]
    private void DeclareObservables()
    {
        IsCharacter = this.WhenPropertyChanged(c => c.CardType)
            .Select(t => t.Value == CardType.Character)
            .AsObservable();

        IsCharacterOrEvent = this.WhenPropertyChanged(c => c.CardType)
            .Select(t => t.Value == CardType.Character || t.Value == CardType.Event)
            .AsObservable();

        FindClimaxCombosCommand = ReactiveCommand.CreateFromTask(FindClimaxCombos);
        FindTraitsCommand = ReactiveCommand.CreateFromTask(FindTraits);
        FindNamesCommand = ReactiveCommand.CreateFromTask(FindNames);

        FindClimaxCombosCommand.ThrownExceptions.Subscribe(ReportException);
        FindTraitsCommand.ThrownExceptions.Subscribe(ReportException);
        FindNamesCommand.ThrownExceptions.Subscribe(ReportException);
    }

    private async Task FindNames()
    {
        if (Parent is null)
            throw new NotImplementedException();

        var newQuery = new NameQueryViewModel(Card);
        await Dispatcher.UIThread.InvokeAsync(() => Parent.SearchQueries.Add(newQuery));
    }

    private async Task FindTraits()
    {
        if (Parent is null)
            throw new NotImplementedException();

        var newQuery = new TraitQueryViewModel(Card);
        await Dispatcher.UIThread.InvokeAsync(() => Parent.SearchQueries.Add(newQuery));
    }

    private async Task FindClimaxCombos()
    {
        if (Parent is null)
            throw new NotImplementedException();

        var newQuery = new ClimaxComboQueryViewModel(Card);
        await Dispatcher.UIThread.InvokeAsync(() => Parent.SearchQueries.Add(newQuery));
    }

    private IEnumerable<Uri> TranslateEffect(string effect)
    {
        var cxcSymbol = new Uri("avares://wsm-gui/Assets/Symbols/cx_combo.png");
        var counter = new Uri("avares://wsm-gui/Assets/Symbols/backup.png");

        // Assume English Text For Now
        if (effect.Contains("CXCOMBO"))
            yield return cxcSymbol;

        if (effect.Contains("[CXC]"))
            yield return cxcSymbol;

        if (effect.Contains("COUNTER"))
            yield return counter;
    }

    internal void Update(WeissSchwarzCard card)
    {
        Card = card;
        Name = card.Name.AsNonEmptyString();
        Traits = card.Traits.Select(t => new MultiLanguageString { EN = t.EN, JP = t.JP }).ToList();
        Serial = card.Serial;
        CardType = card.Type;
        Level = card.Level;
        Cost = card.Cost;
        Power = card.Power;
        Soul = card.Soul;
        EffectMarkers = card.Effect.SelectMany(e => TranslateEffect(e))
            .Distinct()
            .Select(u => Bitmap.DecodeToHeight(AssetLoader.Open(u), 12))
            .ToList();
        Effects = card.Effect.ToList();

        _cachedImageTask = null;
    }


    private void ReportException(Exception exception)
    {
        Log.Error(exception, "Exception Occurred.");

        if (Parent is null)
            return;

        Parent.Status = exception.Message;
    }


}
