namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllTraitCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの《(.+?)》のキャラすべてに、パワーを＋(\d+)。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your <<{trait}>> characters get +{power} power"
            }
        ];
    }
}
