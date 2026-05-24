namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCardFromHandToClockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札の《(.+?)》のキャラを1枚クロック置場に置く(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["手札の《★TESTTRAIT★》のキャラを1枚クロック置場に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 <<{trait}>> character in your hand to your clock"
            }
        ];
    }
}
