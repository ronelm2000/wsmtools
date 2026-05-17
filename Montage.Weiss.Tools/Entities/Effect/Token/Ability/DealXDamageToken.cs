namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Deal X damage equal to soul" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手に X ダメージを与える。X はそのキャラのソウルに等しい。</c></para>
/// <para><b>Regex:</b> ^相手に X ダメージを与える。X はそのキャラのソウルに等しい (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Deal X damage to your opponent. X is equal to that character's soul</c></para>
/// </remarks>
internal class DealXDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手にXダメージを与える。Xはそのキャラのソウルに等しい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Deal X damage to your opponent. X is equal to that character's soul"
            }
        ];
    }
}
