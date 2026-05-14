namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RevealToOpponentToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に見せ(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "reveal it to your opponent"
            }
        ];
    }
}
