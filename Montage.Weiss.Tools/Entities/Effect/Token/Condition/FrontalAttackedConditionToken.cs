namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class FrontalAttackedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードがフロントアタックされた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "When this card is frontal attacked"
            }
        ];
    }
}
