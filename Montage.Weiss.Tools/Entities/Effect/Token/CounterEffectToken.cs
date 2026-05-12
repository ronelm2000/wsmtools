namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class CounterEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【カウンター】\s*(?<mainText>.+)$");

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

        // Translate cost
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? []
            : registry.EffectListRegistry.GetMatch(costTextJapanese)(registry);

        // Check for condition
        var conditionMatch = Regex.Match(rest, @"^(?<condition>.+?)なら、?(?<effect>.+)$");
        string conditionTextJapanese = string.Empty;
        string effectTextJapanese = rest;

        if (conditionMatch.Success)
        {
            conditionTextJapanese = conditionMatch.Groups["condition"].Value + "なら";
            effectTextJapanese = conditionMatch.Groups["effect"].Value.Trim();
        }

        var conditions = string.IsNullOrEmpty(conditionTextJapanese)
            ? []
            : registry.ConditionListRegistry.GetMatch(conditionTextJapanese)(registry);

        // Iterative ability matching
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();
        var remainingText = effectTextJapanese;

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0)
                break;

            if (registry.EffectListRegistry.TryFindFirstMatch(trimmed, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
            {
                if (matchIndex > 0)
                {
                    remainingText = trimmed[matchIndex..];
                    continue;
                }
                var abilList = abilFunc(registry);
                allAbilities.AddRange(abilList);
                abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                remainingText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                remainingText = trimmed.Length > 1 ? trimmed[1..] : "";
            }
        }

        // Post-process opponent references
        if (mainText.Contains("相手の"))
        {
            var opponentRegex = new Regex(@"(?<!\bother\s+)(?<!\byour\s+opponent's\s+)\byour\b(?!\s+opponent's)");
            abilityParts = abilityParts.Select(part =>
                opponentRegex.Replace(part, "your opponent's", 1)
            ).ToList();
        }

        var conditionEnglish = conditions.FirstOrDefault()?.ConditionText ?? "";
        var abilityEnglish = AutoEffectToken.JoinAbilityParts(abilityParts);

        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];

        var effectText = "[COUNTER]";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $" [{costEnglish}]";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        else if (!string.IsNullOrEmpty(costEnglish))
            effectText += " ";
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            if (!string.IsNullOrEmpty(conditionEnglish) && abilityForEffect.Length > 0)
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            effectText += $" {abilityForEffect}";
            if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith('"'))
                effectText += ".";
        }

        return new EventCardEffect
        {
            Labels = ["COUNTER"],
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
