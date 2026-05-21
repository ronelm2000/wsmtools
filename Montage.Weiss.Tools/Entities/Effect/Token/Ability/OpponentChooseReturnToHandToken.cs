namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose opponent characters and return to hand" clauses.
/// Uses <c>your opponent's hand</c> (not <c>their hand</c>) for the return destination.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは相手の前列のキャラを2枚まで選び、手札に戻す</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?相手の(前列の)?キャラを(\d+)枚(?:まで)?選び、手札に戻す(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Optional "前列の" (front row) marker</description></item>
///   <item><description>Group 2: Character count (e.g., "2")</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>choose {up to} N [characters in your opponent's center stage | of your opponent's characters]</c></description></item>
///   <item><description><c>return {it|them} to your opponent's hand</c></description></item>
/// </list>
/// </remarks>
internal class OpponentChooseReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?相手の(前列の)?キャラを(\d+)枚(?:まで)?選び、手札に戻す(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isFrontRow = match.Groups[1].Success;
        var count = int.Parse(match.Groups[2].Value);
        var isUpTo = span.ToString().Contains("まで");
        var countText = isUpTo ? $"up to {count}" : count.ToString();
        var locationText = isFrontRow ? " characters in your opponent's center stage" : " of your opponent's characters";
        var pronoun = count == 1 ? "it" : "them";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText}{locationText}"
            },
            new CardEffectAbility
            {
                AbilityText = $"return {pronoun} to your opponent's hand"
            }
        ];
    }
}
