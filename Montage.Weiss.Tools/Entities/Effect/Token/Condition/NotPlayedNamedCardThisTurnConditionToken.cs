namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NotPlayedNamedCardThisTurnConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このターン中、あなたが「(.+?)」をプレイしていないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have not played \"{name}\" during this turn"
            }
        ];
    }
}
