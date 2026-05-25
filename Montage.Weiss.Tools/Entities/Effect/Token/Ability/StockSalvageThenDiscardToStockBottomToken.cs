namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "salvage N from bottom of stock, then discard same number to bottom of stock" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>ストックの下から2枚までを、手札に戻し、自分の手札を同じ枚数選び、ストック置場の下に好きな順番で置く。</c></para>
/// <para><b>Regex:</b> ^(?:あなたの)?ストックの下から(\d+)枚までを、手札に戻し、自分の手札を同じ枚数選び、ストック置場の下に好きな順番で置(?:く|いてよい|き)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of cards to salvage</description></item>
/// </list>
/// <para><b>Output:</b> returns multiple atomic abilities: salvage, choose discard, put to bottom</para>
/// </remarks>
internal class StockSalvageThenDiscardToStockBottomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたの)?ストックの下から(\d+)枚までを、手札に戻し、自分の手札を同じ枚数選び、ストック置場の下に好きな順番で置(?:く|いてよい|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["ストックの下から2枚までを、手札に戻し、自分の手札を同じ枚数選び、ストック置場の下に好きな順番で置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"return up to {count} cards from the bottom of your stock to your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "choose the same number of cards from your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "put them to the bottom of your stock in any order"
            }
        ];
    }
}
