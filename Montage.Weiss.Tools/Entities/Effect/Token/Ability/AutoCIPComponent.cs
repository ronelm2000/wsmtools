namespace Montage.Weiss.Tools.Entities.Effect.Component.Ability;

internal class AutoCIPComponent : CardTextToken<CardEffect>
{
    /// <summary>
    /// Sample: 【自】［(1) 他のあなたの舞台の、《ファンタジア文庫》か《武器》のキャラを1枚控え室に置く］ このカードが手札から舞台に置かれた時、あなたはコストを払ってよい。そうしたら、あなたは自分の控え室のキャラを1枚選び、手札に戻す。
    /// </summary>
    public override Regex Matcher => new(@"^(?:\【自\】)(?<labels>.?)((?:\［)(?<cost>.?)(?:\］))(?:このカードが手札から舞台に置かれた時、あなたはコストを払ってよい。そうしたら、)(?<effect>.*)");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var effects = registry.EffectListRegistry.GetMatch(match.Groups["effect"].Value)(registry);
        var cost = registry.EffectListRegistry.GetMatch(match.Groups["cost"].Value)(registry);
        var labels = registry.MatchLabels(match.Groups["labels"].Value);
        return new AutoCardEffect
        {
            ConditionText = "When this card is placed on stage from your hand",
            EffectText = string.Empty,
            Cost = cost,
            Condition = [ new() { ConditionText = "When this card is placed on stage from your hand" }],
            Labels = labels,
            Abilities = effects,
            AbilityText = string.Empty
        };
    }
}
