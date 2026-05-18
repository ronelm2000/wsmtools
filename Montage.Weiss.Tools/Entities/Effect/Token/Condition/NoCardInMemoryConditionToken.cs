namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoCardInMemoryConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出置場に「(?<name>.+?)」がないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you do not have \"{name}\" in your memory"
            }
        ];
    }
}
