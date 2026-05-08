namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ReverseConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが【リバース】した時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "When this card becomes [REVERSED]"
            }
        ];
    }
}
