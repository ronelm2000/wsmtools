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
using Avalonia.Media;
using Avalonia.Platform;
using Lamar;


namespace Montage.Weiss.Tools.GUI.ViewModels;

public partial class CardEntryViewModel : ViewModelBase
{
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
    private MultiLanguageString _name;

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
    private Task<Bitmap?> _image;

    [ObservableProperty]
    private List<string> _effects;

    public IObservable<bool> IsCharacter { get; private set; }
    public IObservable<bool> IsCharacterOrEvent { get; private set; }

    public CardEntryViewModel(Uri imageUri, MultiLanguageString name, List<MultiLanguageString> traits)
    {
        DeclareObservables();

        Name = name;
        Traits = traits;
        Serial = "XXX/WS01-100";
        Level = 1;
        Cost = 0;
        Power = 5000;
        Soul = 1;
        CardType = CardType.Character;
        EffectMarkers = new() { Bitmap.DecodeToHeight(AssetLoader.Open(new Uri("avares://wsm-gui/Assets/Symbols/cx_combo.png")), 12) };
        Image = imageUri.Load();
        Effects = ["[AUTO] Aaaaaaaaa"];
    }

    public CardEntryViewModel(WeissSchwarzCard card)
    {
        DeclareObservables();

        Card = card;
        Name = card.Name;
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
        Image = card.LoadImage();
        Effects = card.Effect.ToList();
    }

    private void DeclareObservables()
    {
        IsCharacter = this.WhenPropertyChanged(c => c.CardType)
            .Select(t => t.Value == CardType.Character)
            .AsObservable();

        IsCharacterOrEvent = this.WhenPropertyChanged(c => c.CardType)
            .Select(t => t.Value == CardType.Character || t.Value == CardType.Event)
            .AsObservable();
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
}
