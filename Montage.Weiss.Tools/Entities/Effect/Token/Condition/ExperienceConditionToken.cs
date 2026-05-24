namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ExperienceConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^経験\s*あなたのレベル置場に、「(?<c1>.+?)」と「(?<c2>.+?)」があるなら");

    public override IEnumerable<string> SampleMatches => ["経験 あなたのレベル置場に、「★TESTNAME1★」と「★TESTNAME2★」があるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var c1 = registry.MatchNameFragment(match.Groups["c1"].Value);
        var c2 = registry.MatchNameFragment(match.Groups["c2"].Value);
        
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"\"{c1}\" and \"{c2}\" are in your level"
            }
        ];
    }
}
