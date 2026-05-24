namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class InClockConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのクロック置場に「(?<name>.+?)」があるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,
                ConditionText = $"\"{name}\" is in your clock"
            }
        ];
    }
}
