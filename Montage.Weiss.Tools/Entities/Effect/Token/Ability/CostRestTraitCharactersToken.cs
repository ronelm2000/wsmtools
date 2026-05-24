namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostRestTraitCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの【スタンド】している《(.+?)》のキャラを1枚【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの【スタンド】している《★TESTTRAIT★》のキャラを1枚【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 1 of your other [STAND] <<{trait}>> characters"
            }
        ];
    }
}
