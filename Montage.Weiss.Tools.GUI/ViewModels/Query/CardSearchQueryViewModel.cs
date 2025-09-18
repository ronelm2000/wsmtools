using CommunityToolkit.Mvvm.ComponentModel;
using Montage.Weiss.Tools.Entities;
using System;
using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.GUI.ViewModels;
public abstract partial class CardSearchQueryViewModel : ViewModelBase
{
    [ObservableProperty]
    private QueryType _type;

    [ObservableProperty]
    private string? _displayText;

    [ObservableProperty]
    private string? _toolTip;

    public abstract Func<WeissSchwarzCard, bool> ToPredicate();

    [GeneratedRegex(@"\""(.*)\"" (?:is|in|from)")]
    private static partial Regex ClimaxNameRegex();

    public enum QueryType
    {
        ClimaxCombo,
        Color,
        NeoStandardCode,
        Effect,
        Trait,
        Level,
        NameOrSerial
    }
}
