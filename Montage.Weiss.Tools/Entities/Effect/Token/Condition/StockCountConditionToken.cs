namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class StockCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのストックが(?<count>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If your stock has {count} or more cards"
            }
        ];
    }
}
