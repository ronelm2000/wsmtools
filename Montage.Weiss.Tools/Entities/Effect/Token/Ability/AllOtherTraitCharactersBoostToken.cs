namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOtherTraitCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラすべてに、パワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラすべてに、パワーを＋500。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
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
