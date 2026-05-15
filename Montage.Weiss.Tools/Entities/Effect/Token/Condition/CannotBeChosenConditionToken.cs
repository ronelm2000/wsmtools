namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CannotBeChosenConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードは相手の効果に選ばれない。?");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "This card cannot be chosen by your opponent's effects"
            }
        ];
    }
}
