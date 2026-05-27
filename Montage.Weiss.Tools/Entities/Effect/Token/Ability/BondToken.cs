namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches bond cost clauses: <c>［cost］</c> optionally prefixed by <c>絆／「name」</c>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>絆／「魂魄妖夢」 ［手札を1枚控え室に置く］</c> or standalone <c>［手札を1枚控え室に置く］</c></para>
/// <para><b>Regex:</b> ^(?:絆／「.+?」\s*)?［(.+?)］(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Cost description inside ［］ (e.g., <c>手札を1枚控え室に置く</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>[Put 1 card from your hand to your waiting room]</c></para>
/// <para><b>Label handling:</b> The <c>絆／「name」</c> prefix is consumed by <see cref="AutoEffectToken"/> as a non-bracket label;
/// this token receives only the cost bracket portion or the full text if the prefix wasn't stripped.</para>
/// <para><b>Known cost translations:</b>
/// <list type="bullet">
///   <item><description><c>手札を1枚控え室に置く</c> → <c>Put 1 card from your hand to your waiting room</c></description></item>
/// </list></para>
/// </remarks>
internal class BondToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:絆／「.+?」\s*)?［(.+?)］(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["［手札を1枚控え室に置く］"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costJp = match.Groups[1].Value;
        var costEn = TranslateCost(costJp);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[{costEn}]"
            }
        ];
    }

    private static string TranslateCost(string costJp)
    {
        return costJp switch
        {
            "手札を1枚控え室に置く" => "Put 1 card from your hand to your waiting room",
            _ => costJp
        };
    }
}
