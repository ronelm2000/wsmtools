namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Exchange level with WR" clauses, including the "choose 1 each" variant.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベル置場のカードと控え室のカードを入れ替える</c> or <c>レベル置場のカードと控え室のカードを1枚ずつ選び、入れ替え</c></para>
/// <para><b>Regex:</b> ^レベル置場のカードと控え室のカードを(?:1枚ずつ選び、)?入れ替え(?:る)?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>No named groups — plain text matching with optional <c>1枚ずつ選び</c> segment.</description></item>
/// </list>
/// <para><b>Output (simple variant):</b> <c>Exchange 1 card in your level with 1 card in your waiting room</c></para>
/// <para><b>Output ("choose 1 each" variant, atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>choose 1 card in your level and 1 card in your waiting room</c></description></item>
///   <item><description><c>exchange them</c></description></item>
/// </list>
/// <para><b>Rationale:</b> The "choose each" variant is split into two atomic abilities for proper conjunction handling.</para>
/// </remarks>
internal class ExchangeLevelWithWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^レベル置場のカードと控え室のカードを(?:1枚ずつ選び、)?入れ替え(?:る)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var input = span.ToString();
        var hasChooseEach = input.Contains("1枚ずつ選び");
        if (hasChooseEach)
        {
            return
            [
                new CardEffectAbility { AbilityText = "choose 1 card in your level and 1 card in your waiting room", Prefix = AbilityPrefix.And },
                new CardEffectAbility { AbilityText = "exchange them", Prefix = AbilityPrefix.And }
            ];
        }
        return
        [
            new CardEffectAbility { AbilityText = "Exchange 1 card in your level with 1 card in your waiting room" }
        ];
    }
}
