namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches search-and-place clauses with level- and cost-based filtering (your level / cost 0 or lower / trait).
/// Emits atomic abilities — each returned <see cref="CardEffectAbility"/> represents a single action.
/// The parent (e.g. <see cref="AutoEffectToken"/> via <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/>)
/// joins them with appropriate connectors (", " and ", and ").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札を見て自分のレベル以下でコスト0以下の《NIKKE》のキャラを1枚まで選び、舞台の好きな枠に【レスト】して置き、その山札をシャッフルする。</c></para>
/// <para><b>Regex:</b> ^あなたは自分の山札を見て自分のレベル以下でコスト0以下の《(.+?)》のキャラを1枚まで選び、舞台の好きな枠に【レスト】して置き、その山札をシャッフルする(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output (3 atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description>0: <c>search your deck for up to 1 cost 0 or lower &lt;&lt;NIKKE&gt;&gt; character with level equal to or lower than your level</c></description></item>
///   <item><description>1: <c>put it in any position of your stage as [REST]</c></description></item>
///   <item><description>2: <c>shuffle your deck</c></description></item>
/// </list>
/// <para><b>Atomic Ability Pattern:</b> This token deliberately returns separate <c>CardEffectAbility</c> items
/// rather than one compound string. The parent context joins them, producing consistent connector formatting
/// (", " then ", and "). See README.md "Atomic Ability Pattern" for rationale.</para>
/// </remarks>
internal class SearchDeckLevelAndCostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見て自分のレベル以下でコスト0以下の《(.+?)》のキャラを1枚まで選び、舞台の好きな枠に【レスト】して置き、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to 1 cost 0 or lower <<{trait}>> character with level equal to or lower than your level"
            },
            new CardEffectAbility
            {
                AbilityText = $"put it in any position of your stage as [REST]"
            },
            new CardEffectAbility
            {
                AbilityText = "shuffle your deck"
            }
        ];
    }
}
