namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedMarkerPutOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのマーカーの「(?<name>.+?)」を1枚選び、舞台の好きな枠に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードのマーカーの「凶兆の黒猫 橙」を1枚選び、舞台の好きな枠に置く"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (!match.Success)
            return [];
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 \"{name}\" from this card's markers, and put it on any position on your stage"
            }
        ];
    }
}
