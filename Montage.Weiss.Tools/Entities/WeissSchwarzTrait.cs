using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities;
public class WeissSchwarzTrait : IExactCloneable<WeissSchwarzTrait>, IIdentifiable<Guid>
{
    public Guid TraitID { get; set; }
    public string? EN { get; set; }
    public string? JP { get; set; }

    public string? this[string languageIndex]
    {
        get => (languageIndex == "en") ? EN : JP;
        set
        {
            if (languageIndex == "en")
                EN = value;
            else
                JP = value;
        }
    }

    public Guid Id => TraitID;

    public WeissSchwarzTrait Clone()
    {
        var newMLS = new WeissSchwarzTrait();
        newMLS.TraitID = TraitID;
        newMLS.JP = JP;
        newMLS.EN = EN;
        return newMLS;
    }

    /// <summary>
    /// Attempts to resolve this object into a string as much as it can.
    /// </summary>
    /// <returns></returns>
    public string TraitString => AsNonEmptyString();

    private string AsNonEmptyString()
    {
        StringBuilder sb = new StringBuilder();
        var hasEN = !string.IsNullOrWhiteSpace(EN);
        var hasJP = !string.IsNullOrWhiteSpace(JP);
        if (hasEN && hasJP)
            sb.Append($"{EN} ({JP})");
        else if (hasEN)
            sb.Append(EN);
        else
            sb.Append(JP);
        return sb.ToString();
    }

    public void AssignNewID()
    {
        TraitID = Guid.NewGuid();
    }

    public static Guid Empty => Guid.Empty;
}
