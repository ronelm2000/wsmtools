namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedCardFromHandPutOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?手札の「(.+?)」を(\d+)枚(?:まで)?選び、舞台の好きな枠に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の手札の「★TESTNAME★」を1枚まで選び、舞台の好きな枠に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} \"{name}\" in your hand, and put it on any position on your stage"
            }
        ];
    }
}
