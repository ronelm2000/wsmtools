namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ConditionalPutInHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのカードが《(.+?)》のキャラなら手札に加える?(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["そのカードが《★TESTTRAIT★》のキャラなら手札に加える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"If that card is a <<{trait}>> character, put it to your hand"
            }
        ];
    }
}
