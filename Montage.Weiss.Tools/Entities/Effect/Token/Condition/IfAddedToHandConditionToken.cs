namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class IfAddedToHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^手札に加えたなら(?:、|\.|,|、|。)?");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "you put 1 card to your hand"
            }
        ];
    }
}
