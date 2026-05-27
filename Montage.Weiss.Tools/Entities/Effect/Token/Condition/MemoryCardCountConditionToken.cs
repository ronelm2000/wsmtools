namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches memory card count conditions: "if there are N copies of a specific named card in your memory".
/// Supports both exact count (N枚なら) and "or more" (N枚以上なら) variants.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの思い出置場の「鳥白島四神」が4枚なら</c></para>
/// <para><b>Regex:</b> ^あなたの思い出置場の「(.+?)」が(\d+)枚(?:以上)?なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., "鳥白島四神")</description></item>
///   <item><description>Group 2: Card count (e.g., "4")</description></item>
/// </list>
/// <para><b>Output (exact):</b> <c>you have 4 "鳥白島四神" in your memory</c></para>
/// <para><b>Output (or more):</b> <c>you have 4 or more "鳥白島四神" in your memory</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - 以下 (or less) variants
/// - Combined trait + named card memory count conditions</para>
/// </remarks>
internal class MemoryCardCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出置場の「(.+?)」が(\d+)枚(?:以上)?なら");
    public override IEnumerable<string> SampleMatches => ["あなたの思い出置場の「鳥白島四神」が4枚なら。"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        var isExact = !span.ToString().Contains("以上");
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = isExact ? $"you have {count} \"{name}\" in your memory" : $"you have {count} or more \"{name}\" in your memory"
            }
        ];
    }
}
