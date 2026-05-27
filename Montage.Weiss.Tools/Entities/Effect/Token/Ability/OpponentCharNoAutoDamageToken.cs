namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCharNoAutoDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のキャラの【自】の効果によるダメージを受けない(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you cannot receive damage by the [AUTO] effects of your opponent's characters"
            }
        ];
    }
}
