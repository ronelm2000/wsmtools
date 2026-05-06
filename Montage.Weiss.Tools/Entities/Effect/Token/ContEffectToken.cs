using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ContEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【永】\s*(?<condition>.+)なら、(?<effect>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
        var conditionText = match.Groups["condition"].Value.Trim() + "なら";
        var effectText = match.Groups["effect"].Value.Trim();

        var conditions = registry.ConditionListRegistry.GetMatch(conditionText)(registry);
        var abilities = registry.EffectListRegistry.GetMatch(effectText)(registry);

        var conditionEnglish = conditions.FirstOrDefault()?.ConditionText ?? conditionText;
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        return new ContCardEffect
        {
            Labels = labels,
            ConditionText = conditionEnglish,
            Condition = conditions,
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = $"[CONT] {conditionEnglish}, {abilityEnglish}."
        };
    }
}
