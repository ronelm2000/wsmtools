namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches conditions checking the count of characters in the opponent's center stage.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の前列のキャラが1枚以下なら</c></para>
/// <para><b>Regex:</b> ^(?:相手の)?前列のキャラが(?&lt;count&gt;\d+)枚以下なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Maximum count threshold (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>your opponent's center stage has 1 or less characters</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class OpponentCenterStageCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:相手の)?前列のキャラが(?<count>\d+)枚以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (!match.Success)
            return [];
        var count = int.Parse(match.Groups["count"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"your opponent's center stage has {count} or less characters"
            }
        ];
    }
}
