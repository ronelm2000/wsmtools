namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitFromWrPutToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の《(.+?)》のキャラを(\d+)枚まで選び、ストック置場に(?<anyOrder>好きな順番で)?置く(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = int.Parse(match.Groups[2].Value);
        var anyOrder = match.Groups["anyOrder"].Success;
        var plural = count > 1;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} <<{trait}>> character{(plural ? "s" : "")} in your waiting room, and put {(plural ? "them" : "it")} to your stock{(anyOrder ? " in any order" : "")}"
            }
        ];
    }
}
