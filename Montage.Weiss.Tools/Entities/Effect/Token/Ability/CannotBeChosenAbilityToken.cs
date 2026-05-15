namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CannotBeChosenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは相手の効果に選ばれない。?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "cannot be chosen by your opponent's effects"
            }
        ];
    }
}
