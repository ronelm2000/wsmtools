namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTopOfStockToClockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたの山札の上から1枚をクロック置場に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put the top card of your deck to your clock"
            }
        ];
    }
}
