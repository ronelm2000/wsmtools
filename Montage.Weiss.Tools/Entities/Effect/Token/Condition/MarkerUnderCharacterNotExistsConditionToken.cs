namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if there is no marker under that character" condition clauses.
/// Used for Inheritance (継承) effects that check for marker absence before transferring markers.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのキャラの下にマーカーがないなら</c></para>
/// <para><b>Regex:</b> ^そのキャラの下にマーカーがないなら</para>
/// <para><b>Output:</b> <c>there is no marker under that character</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class MarkerUnderCharacterNotExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^そのキャラの下にマーカーがないなら");

    public override IEnumerable<string> SampleMatches => ["そのキャラの下にマーカーがないなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is no marker under that character"
            }
        ];
    }
}
