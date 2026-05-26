namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "X is equal to ..." post-condition clauses. Detects the optional <c>他の</c> prefix
/// for "your other" vs "your" trait character counts, and appends <c>×N</c> multipliers.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>Ｘは他のあなたの《NIKKE》のキャラの枚数×500に等しい。</c></para>
/// <para><b>Regex:</b> ^[XＸ]\s*は\s*(?&lt;description&gt;.+?)\s*に等しい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "description": Everything between "は" and "に等しい"</description></item>
/// </list>
/// <para><b>Output:</b> <c>X is equal to the number of your [other] &lt;&lt;trait&gt;&gt; characters [×N]</c></para>
/// <para><b>Type:</b> <c>ConditionType.PostCondition</c></para>
/// <para><b>Patterns:</b></para>
/// <list type="bullet">
///   <item><description>公開されたカードのレベル → revealed card level</description></item>
///   <item><description>そのカードのレベル＋1 → that sent card's level +1</description></item>
///   <item><description>そのキャラのソウル → that character's soul</description></item>
///   <item><description>そのキャラのレベル×N → that character's level ×N</description></item>
///   <item><description>それらのカードの {...} → characters put this way</description></item>
///   <item><description>あなたの {...} キャラの枚数[×N] → your [other] &lt;&lt;trait&gt;&gt; characters [×N]</description></item>
///   <item><description>相手の {...} キャラの枚数 → characters your opponent has</description></item>
///   <item><description>控え室に置かれたカードのレベルの合計 → total level of cards put this way</description></item>
///   <item><description>あなたの宣言した数 → the declared number</description></item>
/// </list>
/// </remarks>
internal class XEqualsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^[XＸ]\s*は\s*(?<description>.+?)\s*に等しい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["Xはあなたの《★TESTTRAIT★》のキャラの枚数に等しい。"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var description = match.Groups["description"] is Group g && g.Success ? g.Value : match.Value;
        var translated = description switch
        {
            _ when Regex.IsMatch(description, @"この効果で公開されたカードのレベルの合計が偶数なら\d+、奇数なら\d+") =>
                TranslateParityDescription(description),
            _ when Regex.IsMatch(description, @"このカードのマーカーにカード名に「.+?」を含むキャラがあるなら\d+、ないなら\d+") =>
                $"X is equal to {TranslateMarkerXDescription(description, registry)}",
            _ when description.Contains("公開されたカードのレベル") =>
                "X is equal to the level of the revealed card",
            _ when Regex.IsMatch(description, @"そのカードのレベル[＋+]\d*") =>
                "X is equal to that sent card's level +1",
            _ when description.Contains("そのキャラのソウル") =>
                "X is equal to that character's soul",
            _ when Regex.IsMatch(description, @"そのキャラのレベル[×x]\d+") =>
                $"X is equal to that character's level {Regex.Match(description, @"[×x]\d+").Value}",
            _ when description.Contains("それらのカードの") =>
                $"X is equal to the number of {ExtractTrait(description, registry)} characters put this way",
            _ when description.Contains("あなたの") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of your {(description.Contains("他の") ? "other " : "")}{ExtractTrait(description, registry)} characters{FormatMultiplier(description)}",
            _ when description.Contains("相手の") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of characters your opponent has",
            _ when description.Contains("控え室に置かれたカードのレベルの合計") =>
                "X is equal to the total level of the cards put to your waiting room by this effect",
            _ when description == "あなたの宣言した数" =>
                "X is equal to the declared number",
            _ => description
        };
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.PostCondition,
                ConditionText = translated
            }
        ];
    }

    /// <summary>
    /// Translates a marker-based X definition: "2 if there is a character with 'name' under this card's marker, or 1 if not".
    /// Supports multiple named targets joined by <c>か</c> (e.g., "うみ" or "七海").
    /// </summary>
    private static string TranslateMarkerXDescription(string description, ITokenRegistry registry)
    {
        var ifMatch = Regex.Match(description, @"キャラがあるなら(\d+)");
        var ifNotMatch = Regex.Match(description, @"ないなら(\d+)");
        var ifValue = ifMatch.Success ? ifMatch.Groups[1].Value : "2";
        var ifNotValue = ifNotMatch.Success ? ifNotMatch.Groups[1].Value : "1";
        var names = new List<string>();
        var nameMatches = Regex.Matches(description, @"「(.+?)」");
        foreach (System.Text.RegularExpressions.Match m in nameMatches)
        {
            names.Add(registry.MatchNameFragment(m.Groups[1].Value));
        }
        var nameText = names.Count > 1
            ? string.Join(" or ", names.Select(n => $"\"{n}\""))
            : names.Count == 1 ? $"\"{names[0]}\"" : "?";
        return $"{ifValue} if there is a character with {nameText} in its card name under this card's marker, or {ifNotValue} if not";
    }

    /// <summary>
    /// Translates a parity-based X-definition: "2 if the total level is even, or 1 if it is odd".
    /// Used when X depends on whether the sum of revealed card levels is even or odd.
    /// </summary>
    private static string TranslateParityDescription(string description)
    {
        var evenMatch = Regex.Match(description, @"偶数なら(\d+)");
        var oddMatch = Regex.Match(description, @"奇数なら(\d+)");
        var evenValue = evenMatch.Success ? evenMatch.Groups[1].Value : "2";
        var oddValue = oddMatch.Success ? oddMatch.Groups[1].Value : "1";
        return $"X is equal to {evenValue} if the total level of the cards revealed this way is even, or {oddValue} if it is odd";
    }

    private static string ExtractTrait(string text, ITokenRegistry registry)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? $"<<{registry.MatchNameFragment(match.Groups[1].Value)}>>" : "?";
    }

    private static string FormatMultiplier(string description)
    {
        var multiplierMatch = Regex.Match(description, @"[×x]\d+$");
        return multiplierMatch.Success ? $" {multiplierMatch.Value}" : "";
    }
}
