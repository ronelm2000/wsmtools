namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches hand size threshold conditions.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの手札が5枚以上なら、</c></para>
/// <para><b>Regex:</c> ^あなたの手札が(\d+)枚以上なら、</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Hand count threshold (e.g., "5")</description></item>
/// </list>
/// <para><b>Output:</b> <c>If you have 5 or more cards in your hand</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different thresholds (枚以下なら, 枚なら)
/// - Different hand references (手札の数, 手持ちのカード)</para>
/// </remarks>
internal class HandSizeConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの手札が(\d+)枚以上なら、");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.TryParse(match.Groups[1].Value, out var result) ? result : 0;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If, ConditionText = $"If you have {count} or more cards in your hand"
            }
        ];
    }
}
