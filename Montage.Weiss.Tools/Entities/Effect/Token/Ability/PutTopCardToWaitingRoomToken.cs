namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutTopCardToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札の上から(\d+)枚を、?控え室に置(?:いてよい|く|いて|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var may = match.Value.Contains("てよい");
        var countText = count == 1 ? "card of" : count + " cards of";
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? "you may put the top " + countText + " your deck to your waiting room"
                    : "put the top " + countText + " your deck to your waiting room"
            }
        ];
    }
}
