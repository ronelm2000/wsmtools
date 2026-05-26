namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CompoundMemoryNamedCardAndTraitCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出置場に「(?<name>.+?)」が(?<count1>\d+)枚以上で、他のあなたの《(?<trait>.+?)》のキャラが(?<count2>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        var count1 = match.Groups["count1"].Value;
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count2 = match.Groups["count2"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there are {count1} or more cards named \"{name}\" in your memory, and there are {count2} or more of your other <<{trait}>> characters"
            }
        ];
    }
}
