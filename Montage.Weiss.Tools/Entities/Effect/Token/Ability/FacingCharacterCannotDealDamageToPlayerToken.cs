namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "facing character cannot deal damage to player" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの正面のキャラはプレイヤーにダメージを与えることができない。</c></para>
/// <para><b>Regex:</b> ^このカードの正面のキャラはプレイヤーにダメージを与えることができない(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>the character in front of this card cannot deal damage to a player</c></para>
/// </remarks>
internal class FacingCharacterCannotDealDamageToPlayerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラはプレイヤーにダメージを与えることができない(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードの正面のキャラはプレイヤーにダメージを与えることができない。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "the character in front of this card cannot deal damage to a player"
            }
        ];
    }
}
