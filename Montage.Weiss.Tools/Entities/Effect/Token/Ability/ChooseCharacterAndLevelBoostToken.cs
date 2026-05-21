namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character and level boost" clauses with plural-aware subject-verb agreement.
/// Supports optional "up to" qualifier via the <c>まで</c> marker, singular/plural pronoun selection,
/// and full-width <c>＋</c> for the level value.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>キャラを1枚まで選び、そのターン中、レベルを＋1。</c></para>
/// <para><b>Regex:</b> ^キャラを(\d+)枚(?&lt;upTo&gt;まで)?選び、そのターン中、レベルを[＋\+](\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character count (e.g., "1")</description></item>
///   <item><description>Group "upTo": Optional "まで" marker</description></item>
///   <item><description>Group 2: Level boost value (e.g., "1")</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>choose up to N of your characters</c></description></item>
///   <item><description><c>that character gets +N level until end of turn</c> (singular) / <c>those characters get +N level until end of turn</c> (plural)</description></item>
/// </list>
/// <para><b>Rationale:</b> Split into atomic abilities for proper conjunction handling by the parent token.</para>
/// </remarks>
internal class ChooseCharacterAndLevelBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^キャラを(\d+)枚(?<upTo>まで)?選び、そのターン中、レベルを[＋\+](\d+)(?:\.|,|、|。)?");


    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var isUpTo = match.Groups["upTo"].Success;
        var level = int.Parse(match.Groups[2].Value);
        var upToText = isUpTo ? "up to " : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {upToText}{count} of your characters"
            },
            new CardEffectAbility
            {
                AbilityText = count == 1
                    ? $"that character gets +{level} level until end of turn"
                    : $"those characters get +{level} level until end of turn"
            }
        ];
    }
}
