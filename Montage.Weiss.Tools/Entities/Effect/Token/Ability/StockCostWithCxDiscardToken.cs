namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StockCostWithCxDiscardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)\s*手札のCXを(\d+)枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var stockCost = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"({stockCost}) Put {count} CX in your hand to your waiting room"
            }
        ];
    }
}
