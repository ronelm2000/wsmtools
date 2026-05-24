namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringOpponentTurnAllCharactersAreTraitConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^相手のターン中、あなたのキャラすべてが《(?<trait>.+?)》なら");
    public override IEnumerable<string> SampleMatches => ["相手のターン中、あなたのキャラすべてが《★TESTTRAIT★》なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "your opponent's turn"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"all of your characters are <<{trait}>>"
            }
        ];
    }
}
