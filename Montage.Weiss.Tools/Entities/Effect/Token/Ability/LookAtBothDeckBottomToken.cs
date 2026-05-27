namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class LookAtBothDeckBottomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?山札を下から1枚見て、相手の山札を下から1枚見る(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "look at the bottom card of your deck, and look at the bottom card of your opponent's deck"
            }
        ];
    }
}
