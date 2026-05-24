namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの(?:《(?<trait>.+?)》の)?キャラが(?<count>\d+)枚以上(?:なら|で)");

    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラが2枚以上なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var trait = match.Groups["trait"].Success ? registry.MatchNameFragment(match.Groups["trait"].Value) : null;
        var traitText = trait != null ? $" <<{trait}>>" : "";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have {count} or more other{traitText} characters"
            }
        ];
    }
}
