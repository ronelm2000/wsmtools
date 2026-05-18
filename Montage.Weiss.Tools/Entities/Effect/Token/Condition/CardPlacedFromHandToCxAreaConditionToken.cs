namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "When this card is placed on CX area from hand" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードが手札からCX置場に置かれた時</c></para>
/// <para><b>Regex:</b> ^このカードが手札からCX置場に置かれた時</para>
/// <para><b>Output:</b> <c>When this card is placed on your CX area from your hand</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class CardPlacedFromHandToCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札からCX置場に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is placed on your CX area from your hand"
            }
        ];
    }
}
