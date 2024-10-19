using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
public class TraitQueryViewModel : CardSearchQueryViewModel
{
    private List<string> _traits;

    public TraitQueryViewModel(IEnumerable<string> traits) : base()
    {
        _traits = traits.ToList();

        var traitListString = _traits.ConcatAsString(", ");

        Type = QueryType.Trait;
        DisplayText = traitListString.Limit(10);
        ToolTip = $"Finds any cards with any of the following traits: {traitListString}";
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => c.Traits.Any(t => (t.EN is not null && _traits.Contains(t.EN)) || (t.JP is not null && _traits.Contains(t.JP)));   
    }
}
