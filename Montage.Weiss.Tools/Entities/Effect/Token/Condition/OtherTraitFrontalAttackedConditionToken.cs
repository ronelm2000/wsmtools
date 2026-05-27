namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherTraitFrontalAttackedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(?<trait>.+?)》のキャラがフロントアタックされた時");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《幻想郷》のキャラがフロントアタックされた時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"another of your <<{trait}>> characters is frontal attacked"
            }
        ];
    }
}
