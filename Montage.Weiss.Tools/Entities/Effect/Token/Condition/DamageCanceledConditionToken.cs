namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DamageCanceledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの与えたダメージがキャンセルされた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "When this card's damage is canceled"
            }
        ];
    }
}
