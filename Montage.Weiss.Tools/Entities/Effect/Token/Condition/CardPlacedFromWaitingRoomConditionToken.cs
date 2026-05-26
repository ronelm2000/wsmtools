namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardPlacedFromWaitingRoomConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが控え室から舞台に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is placed on stage from your waiting room"
            }
        ];
    }
}
