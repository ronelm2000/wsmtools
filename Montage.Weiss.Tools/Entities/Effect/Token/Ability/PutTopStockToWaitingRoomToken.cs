namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutTopStockToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^2回行ったら、あなたは自分のストックの上から1枚を、控え室に置く(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the top card of your stock to your waiting room",
                Prefix = AbilityPrefix.AfterThat
            }
        ];
    }
}
