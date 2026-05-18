namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "for each marker underneath this card" condition clauses.
/// Uses a "for each" relationship rather than an "if" check, so no prefix word is emitted.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの下のマーカー1枚につき</c></para>
/// <para><b>Regex:</b> ^このカードの下のマーカー(\d+)枚につき</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: The number before "枚" (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>For each marker underneath this card</c> (the "For each" prefix is supplied by <see cref="ConditionType.For"/>)</para>
/// <para><b>Type:</b> <see cref="ConditionType.For"/> — "For each" prefix prepended by <see cref="CardEffectConditionExtensions.AggregateToString"/></para>
/// <para><b>Scope Expansion:</b> To support different marker counts, no changes needed — the count is embedded in the regex.</para>
/// </remarks>
internal class ThisCardMarkerCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー(\d+)枚につき");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.For,
                ConditionText = $"marker underneath this card"
            }
        ];
    }
}
