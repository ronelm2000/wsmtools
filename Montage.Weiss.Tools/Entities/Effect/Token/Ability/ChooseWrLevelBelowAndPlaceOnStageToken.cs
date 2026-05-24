namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseWrLevelBelowAndPlaceOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^控え室の自分のレベル以下のレベルの《(.+?)》のキャラを(\d+)枚選び、舞台の好きな枠に置く(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["控え室の自分のレベル以下のレベルの《★TESTTRAIT★》のキャラを1枚選び、舞台の好きな枠に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} <<{trait}>> character with level equal to or lower than your level in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"put it in any position of your stage"
            }
        ];
    }
}
