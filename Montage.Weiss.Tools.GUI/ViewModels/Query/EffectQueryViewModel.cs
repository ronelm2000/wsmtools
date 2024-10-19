using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using System;
using System.Linq;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
public partial class EffectQueryViewModel : CardSearchQueryViewModel
{
    private string _effectQuery;

    public EffectQueryViewModel(string effect) : base()
    {
        _effectQuery = effect;

        Type = QueryType.Effect;
        DisplayText = _effectQuery.Limit(10);
        ToolTip = $"Finds any cards with the following ability/effect: {_effectQuery}";
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => c.Effect.Any(e => e.Contains(_effectQuery, StringComparison.OrdinalIgnoreCase));
    }
}
