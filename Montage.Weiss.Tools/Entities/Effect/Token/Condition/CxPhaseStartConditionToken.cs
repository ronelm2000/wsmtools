namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxPhaseStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCXフェイズの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the beginning of your CX phase"
            }
        ];
    }
}
