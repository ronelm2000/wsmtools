namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTopOfStockToClockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの山札の上から 1 枚をクロック置場に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put the top card of your deck to your clock"
            }
        ];
    }
}
