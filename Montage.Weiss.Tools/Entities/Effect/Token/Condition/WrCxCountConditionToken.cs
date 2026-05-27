namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class WrCxCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたか相手の、控え室のCXが(?<count>\d+)枚以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there are {count} or fewer CX in your or your opponent's waiting room"
            }
        ];
    }
}
