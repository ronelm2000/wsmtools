namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnPlacedFromHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれたターン中");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "During the turn this card was placed on stage from the hand"
            }
        ];
    }
}
