namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ClockPhasePlacedFromHandToClockConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのクロックフェイズ中、このカードが手札からクロック置場に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "your clock phase"
            },
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is placed in your clock from your hand"
            }
        ];
    }
}
