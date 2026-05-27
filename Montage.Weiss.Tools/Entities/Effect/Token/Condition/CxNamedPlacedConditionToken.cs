namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxNamedPlacedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:あなたの)?CX置場に「(?<name>(?:「[^」]*」|[^」])+)」が置かれた時");
    public override IEnumerable<string> SampleMatches => ["CX置場に「★TESTNAME★」が置かれた時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
            Type = ConditionType.When,
            ConditionText = $"\"{name}\" is placed on your CX area"
            }
        ];
    }
}
