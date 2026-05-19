namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches damage cancellation trigger conditions.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの与えたダメージがキャンセルされた時</c></para>
/// <para><b>Regex:</c> ^このカードの与えたダメージがキャンセルされた時</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>When this card's damage is canceled</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different phrasing (ダメージがキャンセルされた場合, 無効になった時)
/// - Different subject references (与えたダメージ, 発生したダメージ)</para>
/// </remarks>
internal class DamageCanceledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの与えたダメージがキャンセルされた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "damage dealt by this card is canceled"
            }
        ];
    }
}
