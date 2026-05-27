namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AlarmClockConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^アラーム\s*［.+?］\s*このカードがクロックの1番上にあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "this card is on top of your clock"
            }
        ];
    }
}
