namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ThisCardMarkerUnderneathConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー.*");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "marker underneath this card"
            }
        ];
    }
}
