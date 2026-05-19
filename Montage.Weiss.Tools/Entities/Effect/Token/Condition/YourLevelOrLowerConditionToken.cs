namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches a "your level or lower" qualifier embedded inside a longer effect clause (e.g., a search constraint).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分のレベル以下のレベル</c> (part of a larger search/filter clause)</para>
/// <para><b>Regex:</b> ^自分のレベル以下のレベル</para>
/// <para><b>Output:</b> <c>level equal to or lower than your level</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class YourLevelOrLowerConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^自分のレベル以下のレベル");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "level equal to or lower than your level"
            }
        ];
    }
}
