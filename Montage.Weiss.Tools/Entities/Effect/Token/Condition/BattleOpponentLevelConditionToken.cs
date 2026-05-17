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
    public override Regex Matcher => new(@"^このカードのバトル相手のレベルが (\d+ 以下 |0 以下| 相手のレベルより高い)");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var condition = match.Groups[1].Value;
            var conditionText = condition switch
            {
                "0 以下" => "If this card's battle opponent is level 0 or lower",
                "相手のレベルより高い" => "If this card's battle opponent's level is higher than your level",
                _ => $"If this card's battle opponent is level {condition}"
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
