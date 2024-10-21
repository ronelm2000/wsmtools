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
public partial class TraitQueryViewModel : CardSearchQueryViewModel
{
    private List<string> _traits;
    private static readonly Regex _traitMatcher = TraitMatcher();

    public TraitQueryViewModel(IEnumerable<string> traits) : base()
    {
        _traits = traits.ToList();

        var traitListString = _traits.ConcatAsString(", ");

        Type = QueryType.Trait;
        DisplayText = traitListString.Limit(10);
        ToolTip = $"Finds any cards with any of the following traits: {traitListString}";
    }

    public TraitQueryViewModel(WeissSchwarzCard card) : base()
    {
        _traits =
        [
            .. card.Effect.SelectMany(e => _traitMatcher.Matches(e).Select(m => m.Groups[1].Value)).Distinct()
        ];

        var traitListString = _traits.ConcatAsString(", ");

        Type = QueryType.Trait;
        DisplayText = traitListString.Limit(10);
        ToolTip = $"Finds any cards with any of the following traits: {traitListString}";
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => c.Traits.Any(t => (t.EN is not null && _traits.Contains(t.EN)) || (t.JP is not null && _traits.Contains(t.JP)));   
    }

    [GeneratedRegex(@"(?:\:\:|«|<<|<)(.*?)(?:::|»|>>|>)")]
    private static partial Regex TraitMatcher();
}
