namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class AttackConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードがアタックした時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "When this card attacks"
            }
        ];
    }
}
