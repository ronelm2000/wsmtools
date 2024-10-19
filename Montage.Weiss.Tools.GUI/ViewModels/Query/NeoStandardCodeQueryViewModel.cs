using Montage.Card.API.Utilities;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Montage.Weiss.Tools.GUI.ViewModels.Query;
internal class NeoStandardCodeQueryViewModel : CardSearchQueryViewModel
{
    private List<string> _nsCodesToSearch;

    public NeoStandardCodeQueryViewModel(IEnumerable<string> neoStandardCodes) : base()
    {
        _nsCodesToSearch = neoStandardCodes.ToList();

        var nsCodesToSearchString = _nsCodesToSearch.ConcatAsString(", ");

        Type = QueryType.NeoStandardCode;
        DisplayText = nsCodesToSearchString.Limit(10);
        ToolTip = $"Finds any cards with any of the following Neo-Standard Codes: {nsCodesToSearchString}";
    }

    public override Func<WeissSchwarzCard, bool> ToPredicate()
    {
        return c => _nsCodesToSearch.Any(ns => WeissSchwarzCard.ParseSerial(c.Serial).NeoStandardCode.StartsWith(ns));
    }
}
