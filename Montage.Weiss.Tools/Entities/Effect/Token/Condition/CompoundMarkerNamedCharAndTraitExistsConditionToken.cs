namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CompoundMarkerNamedCharAndTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのマーカーにカード名に「(?<name>.+?)」を含むキャラがあり、他のあなたの《(?<trait>.+?)》のキャラがいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there is a character with \"{name}\" in its card name in this card's markers, and you have another <<{trait}>> character"
            }
        ];
    }
}
