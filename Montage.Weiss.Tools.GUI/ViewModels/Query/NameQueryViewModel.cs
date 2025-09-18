using AngleSharp.Text;
using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
internal partial class NameQueryViewModel : CardSearchQueryViewModel
{
    private List<string> _names;
    private static readonly Regex _nameMatcher = NameMatcher();

    public NameQueryViewModel(WeissSchwarzCard card)
    {
        _names = [
            .. card.Effect.SelectMany(e => _nameMatcher.Matches(e).SelectMany(TranslateNameMatch))
            ];


        var climaxesToSearchString = _names.ConcatAsString(", ");

        Type = QueryType.ClimaxCombo;
        DisplayText = climaxesToSearchString.Limit(10);
        ToolTip = $"Finds any cards with names containing any of the following: {climaxesToSearchString}";
    }

    private IEnumerable<string> TranslateNameMatch(Match match)
    {
        yield return match.Groups[2].Value;
        if (match.Groups[4].Success)
            yield return match.Groups[4].Value;
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => _names.Any(n => (c.Name.EN?.Contains(n, StringComparison.OrdinalIgnoreCase) ?? false) || (c.Name.JP?.Contains(n, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    [GeneratedRegex(@"([""])((?:(?=(?:\\)*)\\.|.)*?)(\((.*)\))?\1")]
    private static partial Regex NameMatcher();
}
