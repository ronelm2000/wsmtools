namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put on top of deck in any order, then put top N cards to stock" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札の上に好きな順番で置き、自分の山札の上から2枚までを、ストック置場に置く。</c></para>
/// <para><b>Regex:</b> ^山札の上に好きな順番で置き、自分の山札の上から(\d+)枚までを、ストック置場に置(?:く|いてよい|き)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of cards to put to stock</description></item>
/// </list>
/// <para><b>Output:</b> <c>put them on top of your deck in any order, and put up to {count} card from the top of your deck to your stock</c></para>
/// </remarks>
internal class PutOnTopThenTopToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札の上に好きな順番で置き、自分の山札の上から(\d+)枚までを、ストック置場に置(?:く|いてよい|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["山札の上に好きな順番で置き、自分の山札の上から2枚までを、ストック置場に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put them on top of your deck in any order, and put up to {count} card from the top of your deck to your stock"
            }
        ];
    }
}
