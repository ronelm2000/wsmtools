namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherCenterStageNamedCharactersConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列の、「(.+?)」と「(.+?)」がいるなら");
    public override IEnumerable<string> SampleMatches => ["他のあなたの前列の、「★TESTNAME1★」と「★TESTNAME2★」がいるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name1 = registry.MatchNameFragment(match.Groups[1].Value);
        var name2 = registry.MatchNameFragment(match.Groups[2].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have another \"{name1}\" and \"{name2}\" in your center stage",
                Conjunction = ConditionConjunction.And
            }
        ];
    }
}
