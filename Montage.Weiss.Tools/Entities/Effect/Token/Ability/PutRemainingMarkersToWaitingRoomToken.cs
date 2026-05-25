namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put the remaining markers to your waiting room" cleanup clauses.
/// Used after marker transfer operations (e.g., Inheritance 継承) to dispose of leftover markers.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>残りのマーカーを控え室に置く。</c></para>
/// <para><b>Regex:</b> ^残りのマーカーを控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>put the remaining markers to your waiting room</c></para>
/// </remarks>
internal class PutRemainingMarkersToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^残りのマーカーを控え室に置く(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["残りのマーカーを控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the remaining markers to your waiting room"
            }
        ];
    }
}
