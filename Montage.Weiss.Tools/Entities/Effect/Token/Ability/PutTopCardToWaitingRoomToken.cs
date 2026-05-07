namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutTopCardToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"控え室に置き");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the top card of your deck into your waiting room"
            }
        ];
    }
}
