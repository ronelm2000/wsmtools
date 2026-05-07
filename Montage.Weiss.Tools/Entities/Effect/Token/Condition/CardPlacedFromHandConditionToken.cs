namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardPlacedFromHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"このカードが手札から舞台に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "When this card is placed on stage from your hand"
            }
        ];
    }
}
