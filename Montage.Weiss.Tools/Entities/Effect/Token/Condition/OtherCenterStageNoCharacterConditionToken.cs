namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherCenterStageNoCharacterConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列のキャラがいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "you have no other characters in your center stage"
            }
        ];
    }
}
