namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class FacingCharacterLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラのレベルが(?<level>\d+)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups["level"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"the character facing this card is level {level}"
            }
        ];
    }
}
