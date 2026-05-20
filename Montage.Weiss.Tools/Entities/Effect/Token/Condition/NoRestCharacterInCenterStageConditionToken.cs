namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoRestCharacterInCenterStageConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列の【レスト】しているキャラがいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "you do not have another [REST] character in your center stage"
            }
        ];
    }
}
