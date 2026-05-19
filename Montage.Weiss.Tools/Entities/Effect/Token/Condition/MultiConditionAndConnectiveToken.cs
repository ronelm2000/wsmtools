namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches the continuative particle `で` used to chain multiple conditions together.
/// Consumed as a separator; contributes the word "and" to join conditions in the output.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>で、</c> (appearing between two condition clauses)</para>
/// <para><b>Regex:</b> ^で、</para>
/// <para><b>Output:</b> <c>and</c> (joins adjacent conditions)</para>
/// <para><b>Type:</b> <c>ConditionType.If</c> (marker only, joined via <c>ConditionConjunction.Continuation</c>)</para>
/// </remarks>
internal class MultiConditionAndConnectiveToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^で、");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "and"
            }
        ];
    }
}
