namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCardAndPutInWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の手札を(\d+)枚選び、控え室に置(?:いて|き|く)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} card{(count > 1 ? "s" : "")} in your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "put it to your waiting room"
            }
        ];
    }
}
