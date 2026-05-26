namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostReductionNamedCardInHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの手札の「(.+?)」のコストを[ー－\-](\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var reduction = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"the cost of \"{name}\" gets -{reduction} in your hand"
            }
        ];
    }
}
