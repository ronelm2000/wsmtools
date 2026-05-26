namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches clock-pick abilities where you choose a character from your clock and put it into waiting room.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分のクロック置場のキャラを1枚まで選び、控え室に置き</c></para>
/// <para><b>Regex:</b> ^自分のクロック置場のキャラを(\d+)枚まで選び、控え室に置き(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Maximum count of characters to choose (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose up to {count} character in your clock, and put it into your waiting room</c></para>
/// <para><b>Scope Expansion:</b> Currently only handles singular character count. If plural count patterns appear,
/// the output text should be updated to handle pluralization (e.g., "characters" vs "character").</para>
/// </remarks>
internal class ChooseClockCharAndPutToWaitingRoomAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分のクロック置場のキャラを(\d+)枚まで選び、控え室に置き(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分のクロック置場のキャラを1枚まで選び、控え室に置き"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} character in your clock, and put it into your waiting room"
            }
        ];
    }
}
