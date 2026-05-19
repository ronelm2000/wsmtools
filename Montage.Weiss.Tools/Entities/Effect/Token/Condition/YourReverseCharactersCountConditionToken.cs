namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches conditions checking the count of your [REVERSE] characters.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの【リバース】しているキャラが3枚以上なら</c></para>
/// <para><b>Regex:</b> ^あなたの【リバース】しているキャラが(\d+)枚以上なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Minimum count threshold (e.g., "3")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you have 3 or more [REVERSE] characters</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class YourReverseCharactersCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの【リバース】しているキャラが(\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have {count} or more [REVERSE] characters"
            }
        ];
    }
}
