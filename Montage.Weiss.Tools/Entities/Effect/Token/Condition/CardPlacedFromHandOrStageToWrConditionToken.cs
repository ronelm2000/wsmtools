namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardPlacedFromHandOrStageToWrConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれた時か舞台から控え室に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card is placed on stage from your hand, or this card is placed from stage to your waiting room"
            }
        ];
    }
}
