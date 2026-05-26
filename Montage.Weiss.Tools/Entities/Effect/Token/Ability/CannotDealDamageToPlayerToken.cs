namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "cannot deal damage to a player" clauses for nested sub-abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードはプレイヤーにダメージを与えることができない。</c></para>
/// <para><b>Regex:</b> ^このカードはプレイヤーにダメージを与えることができない(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>This card cannot deal damage to a player.</c></para>
/// </remarks>
internal class CannotDealDamageToPlayerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードはプレイヤーにダメージを与えることができない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card cannot deal damage to players."
            }
        ];
    }
}
