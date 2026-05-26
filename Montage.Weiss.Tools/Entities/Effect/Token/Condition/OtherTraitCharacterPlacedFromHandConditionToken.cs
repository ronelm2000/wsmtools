namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherTraitCharacterPlacedFromHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラが手札から舞台に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"another of your <<{trait}>> characters is placed on stage from your hand"
            }
        ];
    }
}
