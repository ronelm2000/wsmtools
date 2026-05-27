namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherNamedCxComboDamageNotCanceledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<name>.+?)」の【CXコンボ】の効果で与えたダメージがキャンセルされなかった時");
    public override IEnumerable<string> SampleMatches => ["他のあなたの「楽園の素敵な巫女 霊夢」の【CXコンボ】の効果で与えたダメージがキャンセルされなかった時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"damage dealt by the [CXCOMBO] effect of another of your \"{name}\" is not canceled"
            }
        ];
    }
}
