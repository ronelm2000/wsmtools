namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches damage dealing clauses with X variable definition (level + 1).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手にＸダメージを与える。Ｘはそのカードのレベル＋1に等しい。</c></para>
/// <para><b>Regex:</c> ^相手に[XＸ]\s*ダメージを与える(?:。[XＸ]\s*はそのカードのレベル＋1\s*に等しい(?:\.|,|、|。)?)?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b> None (fixed pattern with optional X definition)</para>
/// <para><b>Output:</b> <c>deal X damage to your opponent. X is equal to that sent card's level +1</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different X definitions (ソウルに等しい, パワーに等しい)
/// - Different damage phrasing (ダメージを与える, ダメージを与える)</para>
/// </remarks>
internal class DealDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に[XＸ]\s*ダメージを与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "deal X damage to your opponent"
            }
        ];
    }
}

/// <summary>
/// Matches damage dealing clauses with X variable definition (soul value).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手にＸダメージを与える。Ｘはそのキャラのソウルに等しい。</c></para>
/// <para><b>Regex:</c> ^相手に[XＸ]\s*ダメージを与える(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>deal X damage to your opponent</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different X definitions (レベルに等しい, コストに等しい)</para>
/// </remarks>
internal class DealVariableDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に[XＸ]\s*ダメージを与える(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "deal X damage to your opponent"
            }
        ];
    }
}
