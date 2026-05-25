namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if that card is a level X or lower character or specific named card" condition clauses.
/// Used for conditional effect triggers based on the type/level of a milled or revealed card.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのカードがレベル2以下のキャラか「蔵で見つけた写真」なら</c></para>
/// <para><b>Regex:</b> ^そのカードがレベル(\d+)以下のキャラか「(.+?)」なら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level threshold (e.g., "2")</description></item>
///   <item><description>Group 2: Named card (e.g., "蔵で見つけた写真")</description></item>
/// </list>
/// <para><b>Output:</b> <c>that card is a level X or lower character or "name"</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class IfThatCardIsLevelOrLowerCharacterOrToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^そのカードがレベル(\d+)以下のキャラか「(.+?)」なら");

    public override IEnumerable<string> SampleMatches => ["そのカードがレベル2以下のキャラか「蔵で見つけた写真」なら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var name = registry.MatchNameFragment(match.Groups[2].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"that card is a level {level} or lower character or \"{name}\""
            }
        ];
    }
}
