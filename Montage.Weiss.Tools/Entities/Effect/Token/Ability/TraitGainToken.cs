namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class TraitGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは《(.+?)》を得る(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードは《★TESTTRAIT★》を得る。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets <<{trait}>>"
            }
        ];
    }
}
