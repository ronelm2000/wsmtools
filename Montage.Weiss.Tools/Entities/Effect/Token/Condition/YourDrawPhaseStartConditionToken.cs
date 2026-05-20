namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class YourDrawPhaseStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?<your>あなたの)?ドローフェイズの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var your = match.Groups["your"].Success ? "your " : "the ";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = $"the beginning of {your}draw phase"
            }
        ];
    }
}
