namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches combined stock cost + rest this card cost clauses (e.g., "(1) [REST] this card").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>(1) 【スタンド】しているこのカードを【レスト】する</c></para>
/// <para><b>Regex:</b> ^\((\d+)\)\s*【スタンド】しているこのカードを【レスト】する(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Stock cost value</description></item>
/// </list>
/// <para><b>Output:</b> <c>({cost}) [REST] this card</c></para>
/// </remarks>
internal class StockCostWithRestThisCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)\s*【スタンド】しているこのカードを【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["(1) 【スタンド】しているこのカードを【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"({cost}) [REST] this card"
            }
        ];
    }
}
