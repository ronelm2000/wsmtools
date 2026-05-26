namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class MarkerExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの下にマーカーがあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is a marker under this card"
            }
        ];
    }
}
