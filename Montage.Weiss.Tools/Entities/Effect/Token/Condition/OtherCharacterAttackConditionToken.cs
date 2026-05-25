namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "your other character attacks" when-condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのキャラがアタックした時</c></para>
/// <para><b>Regex:</b> ^他のあなたのキャラがアタックした時</para>
/// <para><b>Output:</b> <c>When your other character attacks</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class OtherCharacterAttackConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのキャラがアタックした時");
    public override IEnumerable<string> SampleMatches => ["他のあなたのキャラがアタックした時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "your other character attacks"
            }
        ];
    }
}
