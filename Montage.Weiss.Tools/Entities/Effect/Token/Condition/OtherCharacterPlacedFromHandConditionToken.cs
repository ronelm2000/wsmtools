namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "when another of your characters is placed on stage from your hand" condition clauses.
/// Used for Inheritance (継承) effects and other triggered abilities that check for other characters being played.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのキャラが手札から舞台に置かれた時</c></para>
/// <para><b>Regex:</b> ^他のあなたのキャラが手札から舞台に置かれた時</para>
/// <para><b>Output:</b> <c>another of your characters is placed on stage from your hand</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class OtherCharacterPlacedFromHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのキャラが手札から舞台に置かれた時");

    public override IEnumerable<string> SampleMatches => ["他のあなたのキャラが手札から舞台に置かれた時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "another of your characters is placed on stage from your hand"
            }
        ];
    }
}
