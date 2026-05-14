namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutThisCardToStockAndSwapBottomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードをストック置場に置き、あなたのストックの下から 1 枚を、控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put this card to your stock, and put the bottom card of your stock to your waiting room"
            }
        ];
    }
}
