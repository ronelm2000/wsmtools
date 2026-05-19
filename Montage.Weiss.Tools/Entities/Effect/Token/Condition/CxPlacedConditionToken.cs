namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxPlacedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCXがCX置場に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "a CX is placed on your CX area"
            }
        ];
    }
}
