namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:このカードは、)?(?:他の)?あなた(?:に|の)《(.+?)》のキャラがいないなら");
    public override IEnumerable<string> SampleMatches => ["あなたの《★TESTTRAIT★》のキャラがいないなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var isOther = match.Value.StartsWith("他の");
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = isOther ? $"you do not have another <<{trait}>> character" : $"you do not have a <<{trait}>> character"
            }
        ];
    }
}
