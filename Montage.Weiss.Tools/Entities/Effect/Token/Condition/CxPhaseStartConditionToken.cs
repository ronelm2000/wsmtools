namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxPhaseStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCXフェイズの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "At the beginning of your CX phase"
            }
        ];
    }
}
