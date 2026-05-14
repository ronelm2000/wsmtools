namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AnotherTraitNotExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上で、他のあなたの「(?<name>.+?)」がいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups["trait"].Value;
        var count = match.Groups["count"].Value;
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If you have {count} or more other <<{trait}>> characters, and you do not have another \"{name}\""
            }
        ];
    }
}
