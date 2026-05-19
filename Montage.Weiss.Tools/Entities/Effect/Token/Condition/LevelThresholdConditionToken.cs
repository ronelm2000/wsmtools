namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches generic level threshold conditions (above/below).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベルが1以上なら</c></para>
/// <para><b>Regex:</b> ^レベルが(\d+)(以上|以下)なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level value (e.g., "1")</description></item>
///   <item><description>Group 2: Direction — "以上" (or higher) or "以下" (or lower)</description></item>
/// </list>
/// <para><b>Output:</b> <c>level 1 or higher</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class LevelThresholdConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^レベルが(\d+)(以上|以下)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var direction = match.Groups[2].Value;
        var text = direction == "以上" ? "or higher" : "or lower";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"level {level} {text}"
            }
        ];
    }
}
