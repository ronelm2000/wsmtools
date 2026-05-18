namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches reverse condition triggers.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードが【リバース】した時</c></para>
/// <para><b>Regex:</c> ^このカードが【リバース】した時</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>When this card becomes [REVERSE]</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different reverse phrasing (リバースしたとき, 裏返った時)
/// - Different subject references (キャラが【リバース】)</para>
/// </remarks>
internal class ReverseConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードが【リバース】した時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "this card becomes [REVERSE]"
            }
        ];
    }
}
