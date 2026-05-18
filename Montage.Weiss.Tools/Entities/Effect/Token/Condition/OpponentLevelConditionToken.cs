namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OpponentLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのバトル相手のレベルが(\d+)以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"this card's battle opponent is level {level} or lower"
            }
        ];
    }
}
