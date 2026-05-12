namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxNamedPlacedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^CX置場に「(?<name>.+?)」が置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"When \"{name}\" is placed on your CX area"
            }
        ];
    }
}
