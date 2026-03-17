namespace Montage.Card.API.Entities.Impls;

public record MultiLanguageString
{
    private Dictionary<string, string?> resources = new Dictionary<string, string?>();

    public string? this[string languageIndex]
    {
        get { return resources[languageIndex]; }
        set { resources[languageIndex] = value; }
    }

    public string? EN
    {
        get { return resources.GetValueOrDefault("en"); }
        set { resources["en"] = value; }
    }
    public string? JP
    {
        get { return resources.GetValueOrDefault("jp"); }
        set { resources["jp"] = value; }
    }

    public static MultiLanguageString Empty { get; internal set; } = new MultiLanguageString() { EN = "", JP = "" };

    public MultiLanguageString()
    {
        EN = null;
        JP = null;
    }

    /*
    public MultiLanguageString Clone()
    {
        var newMLS = new MultiLanguageString();
        foreach (var val in resources)
            newMLS.resources.Add(val.Key, val.Value);
        return newMLS;
    }
    */

    /// <summary>
    /// Returns an enumerable collection of non-empty string values contained in the resource set.
    /// </summary>
    /// <remarks>This method is used for non-language specific keyword searching when the string in all languages is required.</remarks>
    /// <returns>An enumerable sequence of strings representing the non-empty values from the resource collection. The sequence
    /// may be empty if no values are present.</returns>
    public IEnumerable<string> ToEnumerable()
    {
        return resources.Values.Select(s => s ?? String.Empty).SkipWhile(string.IsNullOrEmpty);
    }

    /// <summary>
    /// Attempts to resolve this object into a string as much as it can.
    /// </summary>
    /// <returns></returns>
    public string AsNonEmptyString()
    {
        StringBuilder sb = new StringBuilder();
        if (EN != null)
            sb.Append(EN);
        else
            sb.Append(JP);

        // if (JP != null)
        //     sb.Append($" ({JP})");
        return sb.ToString();
    }
}
