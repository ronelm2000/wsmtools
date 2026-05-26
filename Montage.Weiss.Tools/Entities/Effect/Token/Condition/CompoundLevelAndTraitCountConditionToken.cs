namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CompoundLevelAndTraitCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのレベルが(?<level>\d+)で、他のあなたの《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups["level"].Value;
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"your level is {level}, and you have {count} or more other <<{trait}>> characters"
            }
        ];
    }
}
