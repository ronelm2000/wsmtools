namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class BattleOpponentCostConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのバトル相手のコストが\s*(?:\d+\s*以下|0\s*以下)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var matchedValue = match.Value;
            var conditionText = matchedValue switch
            {
                _ when matchedValue.Contains("0") && matchedValue.Contains("以下") => "this card's battle opponent is cost 0 or less",
                _ when Regex.IsMatch(matchedValue, @"\d+\s*以下") => $"this card's battle opponent is cost {Regex.Match(matchedValue, @"\d+").Value} or less",
                _ => "this card's battle opponent is cost 0 or less"
            };
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = conditionText
                }
            ];
        }
        return [];
    }
}
