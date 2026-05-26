namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AttackEndWithCxNameCompoundConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのアタックの終わりに、CX置場に「(?<cxName>.+?)」があり、他のあなたの《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上で、このカードの正面のキャラがいないか【リバース】しているなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cxName = registry.MatchNameFragment(match.Groups["cxName"].Value);
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the end of this card's attack"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"\"{cxName}\" is in your CX area"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have {count} or more other <<{trait}>> characters"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is no character facing this card or the character facing this card is [REVERSE]"
            }
        ];
    }
}
