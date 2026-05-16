namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentDeckToWrAndWrToDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは相手の山札の上から1枚を控え室に置き、相手の控え室のカードを1枚選び、山札の上に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the top card of your opponent's deck to their waiting room, choose 1 card in your opponent's waiting room, and put it on the top of their deck"
            }
        ];
    }
}
