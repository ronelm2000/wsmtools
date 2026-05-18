namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのターン中");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "your turn"
            }
        ];
    }
}
