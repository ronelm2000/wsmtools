namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ConditionalPutInHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"そのカードが《(.+?)》のキャラなら手札に加え");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"If that card is a <<{trait}>> character, add it into your hand"
            }
        ];
    }
}
