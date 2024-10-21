using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.GUI.Utilities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
internal class CostQueryViewModel : CardSearchQueryViewModel
{
    private List<int> _costs;

    public CostQueryViewModel(IEnumerable<string> costStrings) : this(
        costStrings.Select(static (str, i) => Parsers.ParseInt(str))
        .Where(i => i is not null)
        .Select(i => i!.Value)
        )
    {
    }

    public CostQueryViewModel(IEnumerable<int> costs) : base()
    {
        _costs = costs.Distinct().ToList();
        if (_costs.Count == 0)
            _costs = [0, 1, 2, 3];   
        var levelStringArray = _costs.Select(i => i.ToString()).ToArray();
        var levelString = levelStringArray.AsReadOnlyMemory().ConcatAsString(",", "or");

        Type = QueryType.Level;
        DisplayText = levelString.Limit(10);
        ToolTip = $"Finds any cost {levelString} cards.";
    }
    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => _costs.Contains(c.Cost ?? 0);
    }
}
