namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AllCharactersAreTraitConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのキャラすべてが《(?<trait>.+?)》なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups["trait"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"all of your characters are <<{trait}>>"
            }
        ];
    }
}
