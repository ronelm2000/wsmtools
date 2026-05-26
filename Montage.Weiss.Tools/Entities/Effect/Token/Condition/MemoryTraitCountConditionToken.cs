namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class MemoryTraitCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出置場の《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there are {count} or more <<{trait}>> characters in your memory"
            }
        ];
    }
}
