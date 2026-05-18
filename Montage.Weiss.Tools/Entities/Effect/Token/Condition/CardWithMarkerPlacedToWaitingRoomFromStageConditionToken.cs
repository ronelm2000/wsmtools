namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardWithMarkerPlacedToWaitingRoomFromStageConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^下にマーカーがあるこのカードが舞台から控え室に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is put to your waiting room from the stage with a marker underneath it"
            }
        ];
    }
}
