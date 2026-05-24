namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AnotherCharacterWithTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(?<trait>.+?)》のキャラがいるなら");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラがいるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have another <<{trait}>> character"
            }
        ];
    }
}
