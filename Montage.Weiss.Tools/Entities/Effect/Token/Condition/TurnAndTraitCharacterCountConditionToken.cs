namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TurnAndTraitCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのターン中、他のあなたの《(.+?)》のキャラが(\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "your turn"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have {count} or more other <<{trait}>> characters"
            }
        ];
    }
}
