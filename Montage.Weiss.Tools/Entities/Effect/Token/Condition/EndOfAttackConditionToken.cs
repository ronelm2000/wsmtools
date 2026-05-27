namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class EndOfAttackConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのアタックの終わりに");
    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the end of this card's attack"
            }
        ];
    }
}
