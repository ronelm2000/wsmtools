namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AnotherSpecificCardExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<name>.+?)」がいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"You have another \"{name}\""
            }
        ];
    }
}
