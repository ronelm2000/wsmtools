namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "declare a number and deal X damage to opponent" clauses.
/// Used for event effects that let the player choose the damage amount by declaring a number.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>好きな数を宣言し、相手にＸダメージを与える。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?好きな数を宣言し、相手にＸダメージを与える(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>declare a number of your choice, deal X damage to your opponent</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Specific number ranges instead of "任意の数"
/// - Multiple damage instances</para>
/// </remarks>
internal class DeclareNumberDealXDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?好きな数を宣言し、相手にＸダメージを与える(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["好きな数を宣言し、相手にＸダメージを与える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "declare a number of your choice, deal X damage to your opponent"
            }
        ];
    }
}
