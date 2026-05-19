namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "Battle opponent level condition" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのバトル相手のレベルが 0 以下なら</c></para>
/// <para><b>Regex:</b> ^このカードのバトル相手のレベルが (\d+ 以下 |0 以下| 相手のレベルより高い)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level condition (e.g., "0 以下", "相手のレベルより高い")</description></item>
/// </list>
/// <para><b>Output:</b> <c>If this card's battle opponent is level 0 or lower</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class BattleOpponentLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのバトル相手のレベルが\s*(?:\d+\s*以下|0\s*以下|相手のレベルより高い)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var matchedValue = match.Value;
            var conditionText = matchedValue switch
            {
                _ when matchedValue.Contains("0") && matchedValue.Contains("以下") => "this card's battle opponent is level 0 or lower",
                _ when Regex.IsMatch(matchedValue, @"\d+\s*以下") => $"this card's battle opponent is level {Regex.Match(matchedValue, @"\d+").Value} or lower",
                _ when matchedValue.Contains("相手のレベルより高い") => "the level of this card's battle opponent is higher than your opponent's level",
                _ => "this card's battle opponent's level is higher"
            };
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = conditionText
                }
            ];
        }
        return [];
    }
}
