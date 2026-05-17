namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class MemoryCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出が(\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"If your memory has {count} or more cards"
            }
        ];
    }
}
