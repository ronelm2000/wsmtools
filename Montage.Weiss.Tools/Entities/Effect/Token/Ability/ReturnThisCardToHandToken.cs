namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnThisCardToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを手札に戻す(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Return this card to your hand"
            }
        ];
    }
}
