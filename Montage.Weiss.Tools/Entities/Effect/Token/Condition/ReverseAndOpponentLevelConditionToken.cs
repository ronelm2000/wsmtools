namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ReverseAndOpponentLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが【リバース】した時、このカードのバトル相手のレベルが(\d+)以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.When,ConditionText = $"When this card becomes [REVERSE], if this card's battle opponent is level {level} or lower"
            }
        ];
    }
}
