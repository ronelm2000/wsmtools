namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches generic level threshold conditions (above/below), with optional <c>あなたの</c> subject prefix.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベルが1以上なら</c> or <c>あなたのレベルが3以上なら</c></para>
/// <para><b>Regex:</b> ^(あなたの)?レベルが(\d+)(以上|以下)なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Optional "あなたの" prefix (success indicates "your level is" form)</description></item>
///   <item><description>Group 2: Level value (e.g., "1", "3")</description></item>
///   <item><description>Group 3: Direction — "以上" (or higher) or "以下" (or lower)</description></item>
/// </list>
/// <para><b>Output (with あなたの):</b> <c>your level is 3 or higher</c></para>
/// <para><b>Output (without あなたの):</b> <c>level 1 or higher</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class LevelThresholdConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(あなたの)?レベルが(\d+)(以上|以下)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var hasYour = match.Groups[1].Success;
        var level = match.Groups[2].Value;
        var direction = match.Groups[3].Value;
        var text = direction == "以上" ? "or higher" : "or lower";
        var prefix = hasYour ? "your level is " : "level ";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"{prefix}{level} {text}"
            }
        ];
    }
}
