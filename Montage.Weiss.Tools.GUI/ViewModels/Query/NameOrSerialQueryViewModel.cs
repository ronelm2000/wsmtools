using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using System;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;

public class NameOrSerialQueryViewModel : CardSearchQueryViewModel
{
    private string _nameOrSerial;

    public NameOrSerialQueryViewModel(string nameOrSerial) : base()
    {
        _nameOrSerial = nameOrSerial;

        Type = QueryType.NameOrSerial;
        DisplayText = _nameOrSerial.Limit(10);
        ToolTip = $"Finds any cards with whose serial or name has the following: {_nameOrSerial}";
    }
    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => (c.Name.EN?.Contains(_nameOrSerial, StringComparison.OrdinalIgnoreCase) ?? false)
            || (c.Name.JP?.Contains(_nameOrSerial, StringComparison.OrdinalIgnoreCase) ?? false)
            || (c.Serial?.Contains(_nameOrSerial, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
