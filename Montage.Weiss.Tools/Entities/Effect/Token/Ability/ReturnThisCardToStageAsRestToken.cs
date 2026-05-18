namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnThisCardToStageAsRestToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはこのカードをこのカードがいた枠に【レスト】して置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may return this card to its previous stage position as [REST]."
            }
        ];
    }
}
