namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches generic "put 1 CX from your CX area to your waiting room" cost (without card name).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのCX置場のCXを1枚控え室に置く</c></para>
/// <para><b>Regex:</b> ^あなたのCX置場のCXを1枚控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>Put 1 CX from your CX area to your waiting room</c></para>
/// <para><b>Contrast:</b> <see cref="CostPutCxFromCxAreaToWaitingRoomToken"/> handles named CX cards with 「」 quotes.</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Named CX cards (handled by <see cref="CostPutCxFromCxAreaToWaitingRoomToken"/>)
/// - Different card count (2枚 instead of 1枚)</para>
/// </remarks>
internal class CostPutGenericCxFromCxAreaToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのCX置場のCXを1枚控え室に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたのCX置場のCXを1枚控え室に置く"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put 1 CX from your CX area to your waiting room"
            }
        ];
    }
}
