namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches search-level-0-or-lower-and-place-on-stage clauses.
/// Emits atomic abilities — each returned <see cref="CardEffectAbility"/> represents a single action.
/// The parent (e.g. <see cref="AutoEffectToken"/> via <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/>)
/// joins them with appropriate connectors (", " then ", and ").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札を見てレベル0以下のキャラを1枚まで選び、舞台の好きな枠に置き、その山札をシャッフルする。</c></para>
/// <para><b>Regex:</b> ^あなたは自分の山札を見てレベル0以下のキャラを(?&lt;count&gt;.+?)枚まで選び、舞台の好きな枠に置き、その山札をシャッフルする(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "count": Number of characters to search (e.g., "1", "X")</description></item>
/// </list>
/// <para><b>Output (3 atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description>0: <c>search your deck for up to 1 level 0 or lower character</c></description></item>
///   <item><description>1: <c>put it in any position of your stage</c></description></item>
///   <item><description>2: <c>shuffle your deck</c></description></item>
/// </list>
/// <para><b>Atomic Ability Pattern:</b> See <c>SearchDeckLevelAndCostToken</c> for the rationale;
/// this token follows the same decomposition pattern.</para>
/// </remarks>
internal class SearchDeckLevelCostAndPlaceToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見てレベル0以下のキャラを(?<count>.+?)枚まで選び、舞台の好きな枠に置き、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility { AbilityText = $"search your deck for up to {count} level 0 or lower character" },
            new CardEffectAbility { AbilityText = "put it in any position of your stage" },
            new CardEffectAbility { AbilityText = "shuffle your deck" }
        ];
    }
}
