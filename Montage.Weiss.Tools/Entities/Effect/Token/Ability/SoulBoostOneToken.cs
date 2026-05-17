namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Soul +1" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>ソウルを＋1。</c></para>
/// <para><b>Regex:</b> ^ソウルを＋1(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>[SOUL] +1</c></para>
/// </remarks>
internal class SoulBoostOneToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^ソウルを＋1(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[SOUL] +1"
            }
        ];
    }
}
