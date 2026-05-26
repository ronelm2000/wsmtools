namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "no other [REST] character in center stage" conditional clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの前列の【レスト】しているキャラがいないなら</c></para>
/// <para><b>Regex:</b> ^他のあなたの前列の【レスト】しているキャラがいないなら</para>
/// <para><b>Output:</b> <c>there is no other [REST] character in your center stage</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class NoRestCharacterInCenterStageConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列の【レスト】しているキャラがいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is no other [REST] character in your center stage"
            }
        ];
    }
}
