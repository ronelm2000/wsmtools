namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedCardFromWrPutAsMarkerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚(?:まで)?選び、このカードの下にマーカーとして(?:好きな順番で)?(?:表向きに|裏向きに)?(?:置いてよい|置く)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        var input = span.ToString();
        var isUpTo = input.Contains("まで");
        var faceUp = input.Contains("表向き") || !input.Contains("裏向き");
        var faceDown = input.Contains("裏向き");
        var anyOrder = input.Contains("好きな順番");
        var plural = count != "1";
        var countText = isUpTo ? $"up to {count}" : count;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText} \"{name}\" in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"put {(plural ? "them" : "it")} {(faceDown ? "face down" : "face up")} under this card as {(plural ? "markers" : "a marker")}{(anyOrder ? " in any order" : "")}"
            }
        ];
    }
}
