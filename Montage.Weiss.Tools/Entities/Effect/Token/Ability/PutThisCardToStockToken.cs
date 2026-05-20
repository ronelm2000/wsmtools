namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutThisCardToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードをストック置場に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put this card to your stock"
            }
        ];
    }
}
