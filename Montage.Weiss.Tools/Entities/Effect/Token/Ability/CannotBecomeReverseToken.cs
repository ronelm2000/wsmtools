namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "cannot become [REVERSE]" clauses for nested sub-abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは【リバース】しない。</c></para>
/// <para><b>Regex:</b> ^このカードは【リバース】しない(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>This card cannot become [REVERSE].</c></para>
/// </remarks>
internal class CannotBecomeReverseToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは【リバース】しない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card cannot become [REVERSE]."
            }
        ];
    }
}
