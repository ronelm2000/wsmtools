namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches conditions checking a card's level against a threshold.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのカードがレベル2以下のキャラなら</c></para>
/// <para><b>Regex:</b> ^そのカードがレベル(\d+)(以上|以下)のキャラなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level value (e.g., "2")</description></item>
///   <item><description>Group 2: Direction — "以上" (or higher) or "以下" (or lower)</description></item>
/// </list>
/// <para><b>Output:</b> <c>that card is a level 2 or lower character</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class CardLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^そのカードがレベル(\d+)(以上|以下)のキャラなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var direction = match.Groups[2].Value;
        var text = direction == "以上" ? "or higher" : "or lower";
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"that card is a level {level} {text} character"
            }
        ];
    }
}
