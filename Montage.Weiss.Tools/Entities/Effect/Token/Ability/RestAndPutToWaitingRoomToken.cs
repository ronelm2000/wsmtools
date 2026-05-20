namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches the cost pattern "[REST] this card & Put this card to your waiting room".
/// Emits two separate abilities so <see cref="ActEffectToken"/> joins them with " &amp; " and capitalizes the second segment.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードを【レスト】し、このカードを控え室に置く</c></para>
/// <para><b>Regex:</b> ^このカードを【レスト】し、このカードを控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Output (two abilities):</b> <c>[REST] this card</c> + <c>put this card to your waiting room</c></para>
/// </remarks>
internal class RestAndPutToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを【レスト】し、このカードを控え室に置く(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[REST] this card"
            },
            new CardEffectAbility
            {
                AbilityText = "put this card to your waiting room"
            }
        ];
    }
}
