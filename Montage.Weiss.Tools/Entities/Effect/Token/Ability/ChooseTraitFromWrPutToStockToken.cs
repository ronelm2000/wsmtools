namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitFromWrPutToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の《(.+?)》のキャラを(\d+)枚まで選び、ストック置場に好きな順番で置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} <<{trait}>> character in your waiting room, and put them to your stock in any order"
            }
        ];
    }
}
