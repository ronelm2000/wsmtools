namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TraitCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラが(\d+)枚以上(?:なら|で)");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラが2枚以上なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,
                ConditionText = $"you have {count} or more other <<{trait}>> characters"
            }
        ];
    }
}
