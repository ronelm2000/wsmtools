namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RevealTopCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の山札の上から1枚を公開(?:し|する)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "reveal the top card of your deck"
            }
        ];
    }
}
