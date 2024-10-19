using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
public partial class ClimaxComboQueryViewModel : CardSearchQueryViewModel
{
    private static readonly Regex climaxRegex = ClimaxNameRegex();
    private List<string> _climaxesToSearch;

    public ClimaxComboQueryViewModel(WeissSchwarzCard card) : base()
    {
        if (card.Type == CardType.Climax)
            _climaxesToSearch = [card.Name.EN];
        else
            _climaxesToSearch = card.Effect.SelectMany(e => climaxRegex.Matches(e))
                .Select(m => m.Groups[1].Value)
                .ToList();

        var climaxesToSearchString = _climaxesToSearch.ConcatAsString(", ");

        Type = QueryType.ClimaxCombo;
        DisplayText = climaxesToSearchString.Limit(10);
        ToolTip = $"Finds any cards with names or abilities referring to any of the following: {climaxesToSearchString}"; 
    }

    public ClimaxComboQueryViewModel(string climaxName) : base()
    {
        _climaxesToSearch = [climaxName];

        var climaxesToSearchString = _climaxesToSearch.ConcatAsString(", ");

        Type = QueryType.ClimaxCombo;
        DisplayText = climaxesToSearchString.Limit(10);
        ToolTip = $"Finds any cards with names or abilities referring to any of the following: {climaxesToSearchString}";
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => c.Effect.Any(e => _climaxesToSearch.Any(cx => e.Contains($"\"{cx}\"")))
            || _climaxesToSearch.Contains(c.Name.EN ?? String.Empty)
            || _climaxesToSearch.Contains(c.Name.JP ?? String.Empty);
    }

    [GeneratedRegex(@"\""(.*)\"" (?:is|in|from)")]
    private static partial Regex ClimaxNameRegex();
}
