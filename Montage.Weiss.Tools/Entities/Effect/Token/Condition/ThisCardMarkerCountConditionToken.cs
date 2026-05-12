namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ThisCardMarkerCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー(\d+)枚につき.*");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"for each marker underneath this card"
            }
        ];
    }
}
