namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class AutoEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【自】(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var mainText = match.Groups["mainText"].Value.Trim();

        // Extract cost if present: ［...］
        var costMatch = Regex.Match(mainText, @"^［(?<cost>.+?)］\s*(?<rest>.+)$");
        string costTextJapanese = string.Empty;
        string rest = mainText;

        if (costMatch.Success)
        {
            costTextJapanese = costMatch.Groups["cost"].Value;
            rest = costMatch.Groups["rest"].Value.Trim();
        }

        // Extract condition: ...時、
        var conditionMatch = Regex.Match(rest, @"^(?<condition>.+?時)、\s*(?<effect>.+)$");
        string conditionTextJapanese = string.Empty;
        string effectTextJapanese = rest;

        if (conditionMatch.Success)
        {
            conditionTextJapanese = conditionMatch.Groups["condition"].Value + "時";
            effectTextJapanese = conditionMatch.Groups["effect"].Value.Trim();
        }

        // Translate cost
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? []
            : registry.EffectListRegistry.GetMatch(costTextJapanese)(registry);

        // Translate condition
        var conditions = string.IsNullOrEmpty(conditionTextJapanese)
            ? []
            : registry.ConditionListRegistry.GetMatch(conditionTextJapanese)(registry);

        // Translate effect
        var abilities = registry.EffectListRegistry.GetMatch(effectTextJapanese)(registry);

        var conditionEnglish = conditions.FirstOrDefault()?.ConditionText ?? conditionTextJapanese;
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));
        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));

        var effectText = "[AUTO]";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $"[{costEnglish}]";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        effectText += $" {abilityEnglish}.";

        return new AutoCardEffect
        {
            Labels = [],
            ConditionText = conditionEnglish,
            Condition = conditions,
            CostText = costEnglish,
            Cost = costAbilities,
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
