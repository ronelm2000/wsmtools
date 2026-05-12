namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class EncoreStepStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのアンコールステップの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "At the beginning of your encore step"
            }
        ];
    }
}
