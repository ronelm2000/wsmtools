namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class BattleOpponentReverseConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのバトル相手が【リバース】した時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "When this card's battle opponent becomes [REVERSE]"
            }
        ];
    }
}
