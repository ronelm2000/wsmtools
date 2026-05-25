namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose the same number of cards from hand and discard" clauses, used after draw effects.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>手札を同じ枚数選び、控え室に置く</c></para>
/// <para><b>Regex:</b> ^手札を同じ枚数選び、控え室に置(?:く|いてよい|き)(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>choose the same number of cards from your hand</c> + <c>put them to your waiting room</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Optional いてよい (you may discard)</para>
/// </remarks>
internal class DrawUpToAndDiscardSameNumberToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札を同じ枚数選び、控え室に置(?:く|いてよい|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["手札を同じ枚数選び、控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose the same number of cards from your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "put them to your waiting room"
            }
        ];
    }
}
