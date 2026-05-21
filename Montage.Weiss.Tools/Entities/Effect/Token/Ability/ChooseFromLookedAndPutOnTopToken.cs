namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose cards from among those looked at, put on top of deck in any order, put rest to waiting room" clauses.
/// Emits three atomic abilities for proper conjunction handling by the parent token.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>カードを2枚まで選び、山札の上に好きな順番で置き、残りのカードを控え室に置き、</c></para>
/// <para><b>Regex:</b> ^カードを(\d+)枚(まで)?選び、山札の上に(?:好きな順番で)?置き、残りのカードを控え室に置(?:く|き)(?:、|\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count (e.g., "2")</description></item>
///   <item><description>Group 2: Optional "まで" (up to) marker</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>choose up to N cards from among them</c></description></item>
///   <item><description><c>put them on the top of your deck in any order</c></description></item>
///   <item><description><c>put the rest to your waiting room</c></description></item>
/// </list>
/// <para><b>Rationale:</b> The regex accepts both dictionary form <c>置く</c> (end of sentence) and continuative <c>置き</c> (chain continues to further abilities like <c>CannotUseActUntilEndOfTurnToken</c>).</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - <c>元の順番で</c> (original order) vs <c>好きな順番で</c> (any order)
/// - Without <c>好きな順番で</c> (default deck-top placement)</para>
/// </remarks>
internal class ChooseFromLookedAndPutOnTopToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^カードを(\d+)枚(まで)?選び、山札の上に(?:好きな順番で)?置き、残りのカードを控え室に置(?:く|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var isPlural = int.TryParse(count, out var n) && n > 1;
        var cardWord = isPlural ? "cards" : "card";
        var themWord = isPlural ? "them" : "it";
        var fullText = span.ToString();
        var orderText = fullText.Contains("好きな順番で") ? " in any order" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} {cardWord} from among them"
            },
            new CardEffectAbility
            {
                AbilityText = $"put {themWord} on the top of your deck{orderText}"
            },
            new CardEffectAbility
            {
                AbilityText = "put the rest to your waiting room"
            }
        ];
    }
}
