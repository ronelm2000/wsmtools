namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches CX-area CX existence conditions in continuative form (used before a comma-separated chain).
/// Uses the same "there is" phrasing as <see cref="CxWithTriggerIconInCxAreaConditionToken"/> for consistency.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>CX置場にCXがあり</c></para>
/// <para><b>Regex:</b> ^CX置場にCXがあり</para>
/// <para><b>Output:</b> <c>there is a CX in your CX area</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class CxAreaHasCxConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^CX置場にCXがあり");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is a CX in your CX area"
            }
        ];
    }
}
