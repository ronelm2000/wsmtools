namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "cannot move to another position" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは他の枠に動かせない。</c></para>
/// <para><b>Regex:</b> ^このカードは他の枠に動かせない</para>
/// <para><b>Output:</b> <c>This card cannot move to another position of the stage.</c></para>
/// </remarks>
internal class CannotMoveToAnotherPositionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは他の枠に動かせない");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card cannot move to another position of the stage."
            }
        ];
    }
}
