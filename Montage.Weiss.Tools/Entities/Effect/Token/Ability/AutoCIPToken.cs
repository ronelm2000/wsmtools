namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AutoCIPToken : CardTextToken<CardEffect>
{
    /// <summary>
    /// Sample: 【自】[手札から舞台に置かれた時] このカードのパワーを＋3000
    /// </summary>
    public override Regex Matcher => new(@"^【自】\[(?<cost>.+?)\]\s*(?<effect>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var costText = match.Groups["cost"].Value;
        var effectText = match.Groups["effect"].Value.Trim();

        var cost = registry.EffectListRegistry.GetMatch(costText)(registry);
        var abilities = registry.EffectListRegistry.GetMatch(effectText)(registry);

        var costEnglish = string.Join(", ", cost.Select(c => c.AbilityText));
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        return new AutoCardEffect
        {
            ConditionText = "When this card is placed on stage from your hand",
            EffectText = $"[AUTO][{costEnglish}] When this card is placed on stage from your hand, {abilityEnglish}.",
            Cost = cost,
            Condition = [new() { ConditionText = "When this card is placed on stage from your hand" }],
            Labels = [],
            Abilities = abilities,
            AbilityText = abilityEnglish
        };
    }
}
