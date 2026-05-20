namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "X is N or higher/lower" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>Ｘが2以上なら</c></para>
/// <para><b>Regex:</b> ^[XＸ]が(\d+)(以上|以下)なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Threshold value (e.g., "2")</description></item>
///   <item><description>Group 2: Direction — "以上" (or higher) or "以下" (or lower)</description></item>
/// </list>
/// <para><b>Output:</b> <c>If, X is 2 or higher</c></para>
/// </remarks>
internal class XThresholdConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^[XＸ]が(\d+)(以上|以下)なら");
    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var threshold = match.Groups[1].Value;
        var direction = match.Groups[2].Value == "以上" ? "or higher" : "or lower";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"X is {threshold} {direction}"
            }
        ];
    }
}
