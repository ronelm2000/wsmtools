namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DamageCancelledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの与えたダメージがキャンセルされた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "when this card's damage is cancelled"
            }
        ];
    }
}
