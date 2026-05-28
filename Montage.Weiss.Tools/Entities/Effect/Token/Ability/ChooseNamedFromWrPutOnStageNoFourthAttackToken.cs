namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedFromWrPutOnStageNoFourthAttackToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^控え室の「(?<name>.+?)」を1枚まで選び、舞台の好きな枠に置き、そのターン中、あなたは4回目以降のアタックができない(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["控え室の「半人半霊 妖夢」を1枚まで選び、舞台の好きな枠に置き、そのターン中、あなたは4回目以降のアタックができない"];

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
                AbilityText = $"choose up to 1 \"{name}\" in your waiting room, put it on any position on your stage, and you cannot perform your 4th or subsequent attack this turn"
            }
        ];
    }
}
