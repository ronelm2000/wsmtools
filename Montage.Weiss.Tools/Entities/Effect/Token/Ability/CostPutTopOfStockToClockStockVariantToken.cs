namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTopOfStockToClockStockVariantToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのストックの上から1枚をクロック置場に置く$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put the top card of your stock to your clock"
            }
        ];
    }
}
