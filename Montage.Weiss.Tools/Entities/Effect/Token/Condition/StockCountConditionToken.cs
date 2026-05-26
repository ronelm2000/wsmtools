namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class StockCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのストックが(?<count>\d+)枚(?<direction>以上|以下)なら");
    public override IEnumerable<string> SampleMatches => ["あなたのストックが3枚以上なら", "あなたのストックが3枚以下なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var isOrLess = match.Groups["direction"].Value == "以下";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = isOrLess
                    ? $"your stock has {count} or fewer cards"
                    : $"your stock has {count} or more cards"
            }
        ];
    }
}
