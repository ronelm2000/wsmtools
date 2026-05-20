namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character and power boost" clauses with plural-aware subject-verb agreement.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分のキャラを1枚選び、そのターン中、パワーを＋1000。</c> or <c>あなたのキャラを2枚選び、そのターン中、パワーを＋1500。</c></para>
/// <para><b>Regex:</b> ^(?:あなたの|自分の)?キャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character count (e.g., "1", "2")</description></item>
///   <item><description>Group 2: Power boost value (e.g., "1000", "1500")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your characters, and that character gets +1000 power until end of turn</c> (singular) / <c>choose 2 of your characters, and those characters get +1500 power until end of turn</c> (plural)</para>
/// </remarks>
internal class ChooseCharacterAndBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたの|自分の)?キャラを(\d+)枚(?<upTo>まで)?選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var isUpTo = match.Groups["upTo"].Success;
        var power = int.Parse(match.Groups[2].Value);
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
                    ? $"that character gets +{power} power until end of turn"
                    : $"those characters get +{power} power until end of turn"
            }
        ];
    }
}
