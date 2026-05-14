namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RestThisCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを【レスト】する(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[REST] this card"
            }
        ];
    }
}
