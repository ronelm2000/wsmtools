namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllTraitCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの《(.+?)》のキャラすべてに、パワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたの《★TESTTRAIT★》のキャラすべてに、パワーを＋500。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
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
