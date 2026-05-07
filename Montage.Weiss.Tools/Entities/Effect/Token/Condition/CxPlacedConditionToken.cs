namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxPlacedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"あなたのCXがCX置場に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                ConditionText = "When your CX is placed into your CX area"
            }
        ];
    }
}
