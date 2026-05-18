namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の控え室のカードを(?<count>.+?)枚まで選び、山札に戻し、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} cards in your waiting room, return them to your deck, and shuffle your deck"
            }
        ];
    }
}
