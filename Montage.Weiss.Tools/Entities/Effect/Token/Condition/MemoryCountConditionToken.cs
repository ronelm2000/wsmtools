namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "your memory has N or more/less cards" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの思い出が3枚以上なら</c></para>
/// <para><b>Regex:</b> ^あなたの思い出が(\d+)枚(以上|以下)なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count threshold</description></item>
///   <item><description>Group 2: Comparison direction ("以上" or "以下")</description></item>
/// </list>
/// <para><b>Output:</b> <c>your memory has N or more cards</c> / <c>your memory has N or less cards</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class MemoryCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出が(\d+)枚(以上|以下)なら");
    public override IEnumerable<string> SampleMatches => ["あなたの思い出が3枚以上なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var isOrLess = match.Groups[2].Value == "以下";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = isOrLess
                    ? $"your memory has {count} or less cards"
                    : $"your memory has {count} or more cards"
            }
        ];
    }
}
