namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StandThisCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを【スタンド】する(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[STAND] this card"
            }
        ];
    }
}
