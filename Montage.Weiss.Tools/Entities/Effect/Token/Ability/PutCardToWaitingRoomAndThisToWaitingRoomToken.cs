namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCardToWaitingRoomAndThisToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札を(\d+) 枚控え室に置き、このカードを.+(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} {(count == 1 ? "card" : "cards")} in your hand to your waiting room, and put this card to your waiting room"
            }
        ];
    }
}
