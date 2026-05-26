namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CharacterIsRestConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが【レスト】しているなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "this card is [REST]"
            }
        ];
    }
}
