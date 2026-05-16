namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnThisCardToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを手札に戻(?:す|してよい)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var text = match.Value;
        var isMay = text.Contains("してよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = isMay
                    ? "you may return this card to your hand"
                    : "Return this card to your hand"
            }
        ];
    }
}
