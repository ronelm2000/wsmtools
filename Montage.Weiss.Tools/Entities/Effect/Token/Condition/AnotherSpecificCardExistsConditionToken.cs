namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AnotherSpecificCardExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<name>.+?)」がいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have another \"{name}\""
            }
        ];
    }
}
