namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnFacingCharacterLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラのレベルが(?<level>\d+)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var level = match.Groups["level"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.During,ConditionText = $"the character facing this card is level {level}"
            }
        ];
    }
}
