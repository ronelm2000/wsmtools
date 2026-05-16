namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class FacingOpponentCharacterConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの正面に相手のキャラがいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "if there is a character facing this card"
            }
        ];
    }
}
