namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches combined "placed on stage from hand or attacks" timing conditions joined by <c>か</c> (or).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードが手札から舞台に置かれた時かアタックした時</c></para>
/// <para><b>Regex:</b> ^このカードが手札から舞台に置かれた時かアタックした時</para>
/// <para><b>Output:</b> <c>this card is placed on stage from your hand or attacks</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// <para><b>Rationale:</b> This token must be registered before <c>CardPlacedFromHandConditionToken</c> so it matches the compound pattern first.
/// The <c>か</c> connector between two timing predicates shares the subject "this card" — the English output merges them via "or".</para>
/// <para><b>Scope Expansion:</b> To support similar <c>か</c>-joined conditions, create additional combined tokens following this pattern:
/// - Identify the two timing predicates joined by <c>か</c>
/// - Produce a single condition text with "or" joining the predicates
/// - Register before the more specific first predicate token</para>
/// </remarks>
internal class CardPlacedFromHandOrAttackConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれた時かアタックした時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is placed on stage from your hand or attacks"
            }
        ];
    }
}
