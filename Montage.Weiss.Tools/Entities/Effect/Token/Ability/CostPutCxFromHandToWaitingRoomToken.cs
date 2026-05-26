namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCxFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札のCXを1枚控え室に置(?:く|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put a CX from your hand to your waiting room"
            }
        ];
    }
}
