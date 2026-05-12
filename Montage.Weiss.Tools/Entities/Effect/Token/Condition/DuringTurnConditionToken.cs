namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのターン中");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "During your turn"
            }
        ];
    }
}
