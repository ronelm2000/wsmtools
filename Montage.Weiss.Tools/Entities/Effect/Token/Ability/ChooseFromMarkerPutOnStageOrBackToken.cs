namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromMarkerPutOnStageOrBackToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?このカードのマーカーの「(.+?)」を1枚選び、舞台の好きな枠に置く\s*/\s*そのキャラを(?:このカードの下に)?マーカーとして表向きに置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 \"{name}\" in this card's markers, and put it in any slot of your stage"
            },
            new CardEffectAbility
            {
                AbilityText = "you may put that character face up underneath this card as a marker"
            }
        ];
    }
}
