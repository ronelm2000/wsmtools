namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedCardFromWrPutOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚(?:まで)?選び、舞台の(?:好きな枠|別々の枠)に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        var input = span.ToString();
        var isUpTo = input.Contains("まで");
        var differentSlots = input.Contains("別々の枠");
        var plural = match.Value.Contains("2枚") || match.Value.Contains("3枚") || (int.TryParse(count, out var c) && c > 1);

        var countText = isUpTo ? $"up to {count}" : count;
        var positionText = differentSlots ? $"on different positions on your stage" : $"on any position on your stage";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText} \"{name}\" in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"put {(plural ? "them" : "it")} {positionText}"
            }
        ];
    }
}
