namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxNamedInCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^CX置場に「(?<name>.+?)」があり");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If \"{name}\" is in your CX area"
            }
        ];
    }
}
