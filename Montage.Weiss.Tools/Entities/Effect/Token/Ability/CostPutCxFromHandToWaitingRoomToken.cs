namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCxFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札のCXを1枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put 1 CX in your hand to your waiting room"
            }
        ];
    }
}
