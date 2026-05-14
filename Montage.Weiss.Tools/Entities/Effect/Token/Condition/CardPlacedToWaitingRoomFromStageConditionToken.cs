namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardPlacedToWaitingRoomFromStageConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが舞台から控え室に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "When this card is put to your waiting room from the stage"
            }
        ];
    }
}
