namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StockCostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"({cost})"
            }
        ];
    }
}
