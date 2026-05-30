namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches hand size threshold conditions (both <c>以上</c> and <c>以下</c>, with continuative <c>で</c> or conditional <c>なら</c>).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの手札が6枚以下で、</c> or <c>あなたの手札が5枚以上なら、</c></para>
/// <para><b>Regex:</b> ^あなたの手札が(\d+)枚(以上|以下)(?:なら|で)(?:、|,)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Hand count threshold (e.g., "5", "6")</description></item>
///   <item><description>Group 2: Comparison direction ("以上" or "以下")</description></item>
/// </list>
/// <para><b>Output (以上):</b> <c>you have 5 or more cards in your hand</c></para>
/// <para><b>Output (以下):</b> <c>your hand has 6 or less cards</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different suffixes (枚以上で、, 枚以下なら、, 枚なら、)
/// - Different hand references (手札の数, 手持ちのカード)
/// - <c>で</c> continuative form used before <c>MultiConditionAndConnectiveToken</c> chaining</para>
/// </remarks>
internal class HandSizeConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの手札が(\d+)枚(以上|以下)(?:なら|で)(?:、|,)?");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.TryParse(match.Groups[1].Value, out var result) ? result : 0;
        var isOrMore = match.Groups[2].Value == "以上";
        var conditionText = isOrMore
            ? $"you have {count} or more cards in your hand"
            : $"your hand has {count} or less cards";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If, ConditionText = conditionText
            }
        ];
    }
}
