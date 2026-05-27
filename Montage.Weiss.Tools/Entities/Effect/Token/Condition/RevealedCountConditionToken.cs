namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if you revealed N or more cards" clauses used after reveal-and-may conditional chains.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>1枚以上公開したなら</c></para>
/// <para><b>Regex:</b> ^(\d+)枚以上公開したなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: The revealed count threshold (e.g., <c>1</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>you revealed N or more cards</c> (as If-type condition)</para>
/// <para><b>Usage:</b> Used in chains like "reveal up to 3 cards, if you revealed 1 or more cards, then..."</para>
/// </remarks>
internal class RevealedCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(\d+)枚以上公開したなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you revealed {count} or more cards"
            }
        ];
    }
}
