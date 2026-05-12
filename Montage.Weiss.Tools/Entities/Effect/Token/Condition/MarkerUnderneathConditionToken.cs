namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class MarkerUnderneathConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^下にマーカーがある.*");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "with a marker underneath it"
            }
        ];
    }
}
