namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxNamedInCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^CX置場に「(?<name>.+?)」があり");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,
                ConditionText = $"\"{name}\" is in your CX area"
            }
        ];
    }
}
