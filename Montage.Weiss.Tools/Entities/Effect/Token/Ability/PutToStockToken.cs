namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"ストック置場に置(?:く|いてよい|き)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var may = match.Value.Contains("てよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = may ? "you may put it to your stock" : "put it to your stock"
            }
        ];
    }
}
