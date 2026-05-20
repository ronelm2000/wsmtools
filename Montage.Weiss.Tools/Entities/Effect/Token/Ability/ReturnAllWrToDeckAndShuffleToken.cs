namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnAllWrToDeckAndShuffleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の控え室のカードすべてを、山札に戻してよい。そうしたら、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may return all cards in your waiting room to your deck. If you do, shuffle your deck"
            }
        ];
    }
}
