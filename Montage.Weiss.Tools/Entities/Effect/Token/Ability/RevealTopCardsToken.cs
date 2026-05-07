namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RevealTopCardsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"山札の上から(\d+)枚をめくり、控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Flip over {count} cards from the top of your deck, and put them into your waiting room"
            }
        ];
    }
}
