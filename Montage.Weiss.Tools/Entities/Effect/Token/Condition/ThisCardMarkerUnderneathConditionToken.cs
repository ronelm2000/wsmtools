namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ThisCardMarkerUnderneathConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー.*");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "marker underneath this card"
            }
        ];
    }
}
