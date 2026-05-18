namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OpponentAttackPhaseStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^相手のアタックフェイズの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the beginning of your opponent's attack phase"
            }
        ];
    }
}
