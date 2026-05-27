namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CardPlacedFromHandOrAutoEffectConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれた時か「(?<name>.+?)」の【自】の効果で舞台に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"this card is placed on stage from your hand, or this card is placed on stage by the [AUTO] effect of \"{name}\""
            }
        ];
    }
}
