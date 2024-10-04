using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.GUI.Extensions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels;
public partial class CardRatioViewModel : ViewModelBase
{
    [ObservableProperty]
    WeissSchwarzCard _card;

    [ObservableProperty]
    private int _ratio;

    [ObservableProperty]
    private Task<Bitmap?> _image;

    [ObservableProperty]
    private List<string> _effects;

    public IObservable<bool> IsTwoOrMore { get; private set; }
    public IObservable<bool> IsThreeOrMore { get; private set; }
    public IObservable<bool> IsFourOrMore { get; private set; }
    public IObservable<int> RatioMaxWidth { get; private set; }
    public IObservable<Thickness> FirstCardAdjustment { get; private set; }
    public IObservable<int> ImageAngle { get; private set; }

    public CardRatioViewModel()
    {
        Card ??= new WeissSchwarzCard();
        if (Design.IsDesignMode)
        {
            Ratio = 4;
            Image = new Uri("avares://wsm-gui/Assets/Samples/sample_card.jpg").Load();
            Effects = ["[AUTO] aaaaaaaaaaaaaaa"];
        }

        IsTwoOrMore = this.WhenPropertyChanged(r => r.Ratio)
            .Select(r => r.Value >= 2)
            .AsObservable();
        IsThreeOrMore = this.WhenPropertyChanged(r => r.Ratio)
            .Select(r => r.Value >= 3)
            .AsObservable();
        IsFourOrMore = this.WhenPropertyChanged(r => r.Ratio)
            .Select(r => r.Value >= 4)
            .AsObservable();
        RatioMaxWidth = this.WhenAnyValue(
            r => r.Ratio,
            r => r.Card,
            (ratio, card) => (card?.Type == CardType.Climax) ? (180 + ratio * 10) : (145 + ratio * 10)
            );
        FirstCardAdjustment = this.WhenPropertyChanged(r => r.Ratio)
            .Select(r => new Thickness(r.Value * 5, 0, 0, 0))
            .AsObservable();
        ImageAngle = this.WhenPropertyChanged(c => c.Card)
            .Select(c => c.Value?.Type == CardType.Climax ? 270 : 0)
            .AsObservable();

    }

    public CardRatioViewModel(WeissSchwarzCard card, int ratio) : this()
    {
        Card = card;
        Ratio = ratio;
        Image = card.LoadImage();
        Effects = card.Effect.ToList();
    }
}
