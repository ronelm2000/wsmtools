namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OpponentLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのバトル相手のレベルが(\d+)以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var level = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                ConditionText = $"if this card's battle opponent is level {level} or lower"
            }
        ];
    }
}
