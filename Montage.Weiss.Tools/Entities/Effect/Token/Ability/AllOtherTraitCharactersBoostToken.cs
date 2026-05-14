namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOtherTraitCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラすべてに、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your other <<{trait}>> characters get +{power} power"
            }
        ];
    }
}
