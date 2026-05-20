namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentWrCardToTopOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは相手の控え室のカードを(\d+)枚選び、山札の上に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var countText = count == 1 ? "1 card" : $"{count} cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {countText} in your opponent's waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = "put it on the top of their deck"
            }
        ];
    }
}
