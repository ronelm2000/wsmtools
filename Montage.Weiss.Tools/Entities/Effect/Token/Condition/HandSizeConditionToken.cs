namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class HandSizeConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの手札が(\d+)枚以上なら、");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If, ConditionText = $"If you have {count} or more cards in your hand"
            }
        ];
    }
}
