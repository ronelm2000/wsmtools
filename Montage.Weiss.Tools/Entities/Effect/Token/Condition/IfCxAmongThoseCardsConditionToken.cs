namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class IfCxAmongThoseCardsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^それらのカードにCXがあるなら(?:、|\.|,|、|。)?");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is a CX among those cards"
            }
        ];
    }
}
