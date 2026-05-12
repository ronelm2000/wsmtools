namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TurnOnceConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^【ターン1】");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "[1/TURN]"
            }
        ];
    }
}
