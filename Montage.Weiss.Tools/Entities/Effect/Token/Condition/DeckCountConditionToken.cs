namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches deck size threshold conditions (at or below a count).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの山札が6枚以下なら</c></para>
/// <para><b>Regex:</b> ^あなたの山札が(\d+)枚以下なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count threshold (e.g., "6")</description></item>
/// </list>
/// <para><b>Output:</b> <c>your deck has 6 or less cards</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class DeckCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの山札が(\d+)枚以下なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"your deck has {count} or less cards"
            }
        ];
    }
}
