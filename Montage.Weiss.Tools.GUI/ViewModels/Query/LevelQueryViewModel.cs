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
internal class LevelQueryViewModel : CardSearchQueryViewModel
{
    private List<int> _levels;

    public LevelQueryViewModel(IEnumerable<string> levelStrings) : this(
        levelStrings.Select(static (str, i) => Parsers.ParseInt(str))
        .Where(i => i is not null)
        .Select(i => i!.Value)
        )
    {
    }

    public LevelQueryViewModel(IEnumerable<int> levels) : base()
    {
        _levels = levels.Distinct().ToList();
        if (_levels.Count == 0)
            _levels = [0, 1, 2, 3];   
        var levelStringArray = _levels.Select(i => i.ToString()).ToArray();
        var levelString = levelStringArray.AsReadOnlyMemory().ConcatAsString(",", "or");

        Type = QueryType.Level;
        DisplayText = levelString.Limit(10);
        ToolTip = $"Finds any level {levelString} cards.";
    }
    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => _levels.Contains(c.Level ?? 0);
    }
}
