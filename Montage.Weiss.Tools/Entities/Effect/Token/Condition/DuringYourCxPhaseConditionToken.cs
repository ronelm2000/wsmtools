namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringYourCxPhaseConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCXフェイズ中、");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "your CX phase"
            }
        ];
    }
}
