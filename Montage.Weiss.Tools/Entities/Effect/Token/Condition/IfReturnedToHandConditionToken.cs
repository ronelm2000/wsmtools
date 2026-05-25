namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if you returned N cards to your hand" condition clauses.
/// Used for CXCOMBO follow-up effects that check how many cards were retrieved before deciding the next action.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>2枚手札に戻したなら</c></para>
/// <para><b>Regex:</b> ^(?&lt;count&gt;\d+)枚手札に戻したなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Number of cards returned (e.g., "2")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you returned N cards</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class IfReturnedToHandConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?<count>\d+)枚手札に戻したなら");

    public override IEnumerable<string> SampleMatches => ["2枚手札に戻したなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you returned {count} cards"
            }
        ];
    }
}
