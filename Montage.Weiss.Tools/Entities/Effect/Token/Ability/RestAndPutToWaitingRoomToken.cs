namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RestAndPutToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカードを【レスト】し、このカードを控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[REST] this card, and put this card to your waiting room"
            }
        ];
    }
}
