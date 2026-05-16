namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DamageNotCanceledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの与えたダメージがキャンセルされなかった時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "When damage dealt by this card is not canceled"
            }
        ];
    }
}
