namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnPlacedFromHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれたターン中");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "the turn that this card is placed on the stage in your hand"
            }
        ];
    }
}
