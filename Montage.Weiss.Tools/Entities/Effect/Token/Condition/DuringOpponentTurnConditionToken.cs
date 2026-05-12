namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringOpponentTurnConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^相手のターン中");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "During your opponent's turn"
            }
        ];
    }
}
