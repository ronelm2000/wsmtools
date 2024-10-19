using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
public partial class ColorQueryViewModel : CardSearchQueryViewModel
{
    private List<CardColor> _colors;

    public ColorQueryViewModel(IEnumerable<string> colors) : base()
    {
        _colors = colors.Select(TranslateColor).ToList();

        var colorsToSearchString = _colors.Select(static c => c.ToString()).ConcatAsString(", ");

        Type = QueryType.Color;
        DisplayText = colorsToSearchString.Limit(10);
        ToolTip = $"Finds any cards with any of the following colors: {colorsToSearchString}"; 
    }

    private CardColor TranslateColor(string rawColorString) => rawColorString.ToLower() switch
    {
        "y" or "yellow" => CardColor.Yellow,
        "g" or "green" => CardColor.Green,
        "r" or "red" => CardColor.Red,
        "b" or "blue" => CardColor.Blue,
        "p" or "purple" => CardColor.Purple,
        _ => CardColor.Purple
    };

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => _colors.Contains(c.Color);
    }
}
