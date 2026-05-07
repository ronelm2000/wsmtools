namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class AutoEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【自】(?<labels>【.+?】)?\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
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

        // Extract conditions from the beginning
        var conditionsJapanese = new List<string>();
        var effectTextJapanese = rest;

        // Split by 、 and process each part
        var parts = effectTextJapanese.Split('、', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();

        var remainingParts = new List<string>();
        var allConditionsFound = true;
        foreach (var part in parts)
        {
            if (allConditionsFound)
            {
                try
                {
                    // Try to match as a condition
                    var condList = registry.ConditionListRegistry.GetMatch(part)(registry);
                    conditionsJapanese.Add(part);
                }
                catch (NotImplementedException)
                {
                    // This part is not a condition, so stop looking for conditions
                    allConditionsFound = false;
                    remainingParts.Add(part);
                }
            }
            else
            {
                remainingParts.Add(part);
            }
        }

        effectTextJapanese = string.Join("、", remainingParts);

        // Translate cost
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? []
            : registry.EffectListRegistry.GetMatch(costTextJapanese)(registry);

        // Translate conditions
        var conditions = new List<CardEffectCondition>();
        foreach (var cond in conditionsJapanese)
        {
            try
            {
                var condList = registry.ConditionListRegistry.GetMatch(cond)(registry);
                conditions.AddRange(condList);
            }
            catch (NotImplementedException)
            {
                // Ignore conditions that don't match
            }
        }

        // Translate effects - split by 。and then by 、
        var sentenceAbilities = new List<List<CardEffectAbility>>();
        var allAbilities = new List<CardEffectAbility>();
        if (!string.IsNullOrEmpty(effectTextJapanese))
        {
            var sentences = effectTextJapanese.Split('。', StringSplitOptions.RemoveEmptyEntries);
            foreach (var sentence in sentences)
            {
                var sentenceTrimmed = sentence.Trim();
                if (string.IsNullOrEmpty(sentenceTrimmed))
                    continue;

                var sentenceAbilityList = new List<CardEffectAbility>();

                // Split by 、 and try each part
                var subParts = sentenceTrimmed.Split('、', StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPart in subParts)
                {
                    var partTrimmed = subPart.Trim();
                    if (!string.IsNullOrEmpty(partTrimmed))
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to match part: {partTrimmed}");
                        try
                        {
                            var partAbilities = registry.EffectListRegistry.GetMatch(partTrimmed)(registry);
                            System.Diagnostics.Debug.WriteLine($"Matched! Got {partAbilities.Count} abilities");
                            sentenceAbilityList.AddRange(partAbilities);
                            allAbilities.AddRange(partAbilities);
                        }
                        catch (NotImplementedException)
                        {
                            System.Diagnostics.Debug.WriteLine($"No match found for: {partTrimmed}");
                            // Ignore parts that don't match
                        }
                    }
                }

                if (sentenceAbilityList.Count > 0)
                {
                    sentenceAbilities.Add(sentenceAbilityList);
                }
            }
        }

        var conditionEnglish = string.Join(", ", conditions.Select(c => c.ConditionText).Where(c => !string.IsNullOrEmpty(c)));
        
        // Join abilities by sentence, then join sentences with periods
        // Each sentence's abilities are joined with Oxford comma: "A, B, and C"
        var sentenceTexts = new List<string>();
        foreach (var sentenceAbilityList in sentenceAbilities)
        {
            var abilityTexts = sentenceAbilityList.Select(a => a.AbilityText).Where(a => !string.IsNullOrEmpty(a)).ToList();
            string sentenceAbilityEnglish;
            if (abilityTexts.Count == 0)
            {
                continue;
            }
            else if (abilityTexts.Count == 1)
            {
                sentenceAbilityEnglish = abilityTexts[0];
            }
            else if (abilityTexts.Count == 2)
            {
                sentenceAbilityEnglish = $"{abilityTexts[0]}, and {abilityTexts[1]}";
            }
            else
            {
                var allButLast = string.Join(", ", abilityTexts.Take(abilityTexts.Count - 1));
                sentenceAbilityEnglish = $"{allButLast}, and {abilityTexts[^1]}";
            }
            sentenceTexts.Add(sentenceAbilityEnglish);
        }
        
        var abilityEnglish = string.Join(". ", sentenceTexts);
        // Capitalize first letter of AbilityText
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            abilityEnglish = char.ToUpper(abilityEnglish[0]) + abilityEnglish[1..];
        }
        System.Diagnostics.Debug.WriteLine($"Final abilityEnglish: {abilityEnglish}");
        
        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));

        var labelPrefix = labels.Length > 0 ? $"[{string.Join("][", labels)}]" : "";
        var effectText = $"[AUTO]{labelPrefix}";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $"[{costEnglish}]";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        else if (!string.IsNullOrEmpty(costEnglish))
            effectText += " ";
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            // Lowercase first letter if it follows a condition
            if (!string.IsNullOrEmpty(conditionEnglish) && abilityForEffect.Length > 0)
            {
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            }
            effectText += $" {abilityForEffect}";
            if (!abilityForEffect.EndsWith('.'))
                effectText += ".";
        }

        return new AutoCardEffect
        {
            Labels = labels,
            ConditionText = conditionEnglish,
            Condition = conditions,
            CostText = costEnglish,
            Cost = costAbilities,
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
