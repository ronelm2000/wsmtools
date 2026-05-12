namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RevealTopCardWithPrefixToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札の上から(\d+)枚を公開(?:し|する)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "reveal the top card of your deck"
            }
        ];
    }
}
