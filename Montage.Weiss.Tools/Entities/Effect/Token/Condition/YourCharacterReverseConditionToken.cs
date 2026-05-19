namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches when-conditions for one of YOUR characters becoming [REVERSE] (as opposed to this card).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのキャラが【リバース】した時</c></para>
/// <para><b>Regex:</b> ^あなたのキャラが【リバース】した時</para>
/// <para><b>Output:</b> <c>your character becomes [REVERSE]</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class YourCharacterReverseConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのキャラが【リバース】した時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "your character becomes [REVERSE]"
            }
        ];
    }
}
