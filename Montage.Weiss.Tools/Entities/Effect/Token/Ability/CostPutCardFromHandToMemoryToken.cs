namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCardFromHandToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札を1枚控え室に置き、このカードを思い出にする");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put 1 card in your hand to your waiting room & Put this card to your memory"
            }
        ];
    }
}
