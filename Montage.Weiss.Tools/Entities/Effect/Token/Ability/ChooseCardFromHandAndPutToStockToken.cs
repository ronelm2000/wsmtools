namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a card from your hand and put it to your stock" clauses.
/// Used for post-action effects after returning cards to hand (CXCOMBO cleanup).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>手札を1枚選び、ストック置場に置いてよい。</c></para>
/// <para><b>Regex:</b> ^手札を(\d+)枚選び、ストック置場に置いてよい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may choose 1 card from your hand, and put it to your stock</c></para>
/// </remarks>
internal class ChooseCardFromHandAndPutToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札を(\d+)枚選び、ストック置場に置いてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["手札を1枚選び、ストック置場に置いてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {count} card from your hand, and put it to your stock"
            }
        ];
    }
}
