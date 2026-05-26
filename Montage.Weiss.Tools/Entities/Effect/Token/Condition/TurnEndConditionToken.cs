namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "at the end of the turn" temporal conditions.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>ターンの終わりに</c></para>
/// <para><b>Regex:</b> ^ターンの終わりに</para>
/// <para><b>Captures:</b> None (fixed text)</para>
/// <para><b>Output:</b> <c>the end of the turn</c></para>
/// <para><b>Type:</b> <c>ConditionType.At</c> — <see cref="CardEffectConditionExtensions.AggregateToString"/> prepends "At".</para>
/// <para><b>Note:</b> The <c>ConditionText</c> is "the end of the turn" (without "At") because
/// <see cref="CardEffectConditionExtensions.AggregateToString"/> prepends the "At" prefix from <see cref="ConditionType.At"/>.
/// Including "At" in the text would produce double "At at the end of the turn".</para>
/// </remarks>
internal class TurnEndConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^ターンの終わりに");

    public override IEnumerable<string> SampleMatches => ["ターンの終わりに"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the end of the turn"
            }
        ];
    }
}
