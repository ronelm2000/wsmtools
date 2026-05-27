namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if there are other characters of yours" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのキャラがいるなら</c></para>
/// <para><b>Regex:</b> ^他のあなたのキャラがいるなら</para>
/// <para><b>Output:</b> <c>you have another character</c> (as If-type condition)</para>
/// <para><b>Inversion:</b> This is the positive counterpart of <see cref="NoOtherCharacterExistsConditionToken"/>
/// which matches <c>他のあなたのキャラがいないなら</c> (if there are no other characters).</para>
/// </remarks>
internal class AnotherCharacterExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのキャラがいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "you have another character"
            }
        ];
    }
}
