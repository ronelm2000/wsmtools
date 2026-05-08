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

        // Check for ASCII bracket shorthand condition like [手札から舞台に置かれた時]
        var asciiConditionMatch = Regex.Match(rest, @"^\[(?<condition>.+?)\]\s*(?<remaining>.+)$");
        string asciiConditionJapanese = string.Empty;
        if (asciiConditionMatch.Success)
        {
            asciiConditionJapanese = asciiConditionMatch.Groups["condition"].Value;
            rest = asciiConditionMatch.Groups["remaining"].Value.Trim();
        }

        // Translate cost
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? []
            : registry.EffectListRegistry.GetMatch(costTextJapanese)(registry);

        // Iterative condition matching using ^-anchored condition tokens
        var conditions = new List<CardEffectCondition>();
        var remainingText = rest;

        // Parse ASCII-bracket condition if present
        if (!string.IsNullOrEmpty(asciiConditionJapanese))
        {
            try
            {
                var condList = registry.ConditionListRegistry.GetMatch(asciiConditionJapanese)(registry);
                conditions.AddRange(condList);
            }
            catch (NotImplementedException)
            {
            }
        }

        // Iteratively consume conditions from start of remaining text
        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (registry.ConditionListRegistry.TryMatchAtStart(trimmed, out var condFunc, out var consumed) && condFunc != null)
            {
                var condList = condFunc(registry);
                conditions.AddRange(condList);
                remainingText = trimmed[consumed..].TrimStart('、', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        // Iterative ability matching using TryFindFirstMatch (matches anywhere, not just at start)
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0)
                break;

            if (registry.EffectListRegistry.TryFindFirstMatch(trimmed, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
            {
                if (matchIndex > 0)
                {
                    // Skip non-matching prefix (e.g., "あなたは自分の")
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

        var conditionTexts = conditions.Select(c => c.ConditionText).Where(c => !string.IsNullOrEmpty(c)).ToList();
        for (int i = 1; i < conditionTexts.Count; i++)
        {
            if (conditionTexts[i].Length > 0)
                conditionTexts[i] = char.ToLower(conditionTexts[i][0]) + conditionTexts[i][1..];
        }
        var conditionEnglish = string.Join(", ", conditionTexts);

        var abilityEnglish = JoinAbilityParts(abilityParts);

        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];

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
            if (!string.IsNullOrEmpty(conditionEnglish) && abilityForEffect.Length > 0)
            {
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            }
            effectText += $" {abilityForEffect}";
            if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith(']'))
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

    internal static string JoinAbilityParts(List<string> parts)
    {
        if (parts.Count == 0)
            return "";

        // Group by sentence: uppercase start = new sentence
        var groups = new List<List<string>>();
        var currentGroup = new List<string> { parts[0] };
        for (int i = 1; i < parts.Count; i++)
        {
            if (parts[i].Length > 0 && char.IsUpper(parts[i][0]))
            {
                groups.Add(currentGroup);
                currentGroup = new List<string> { parts[i] };
            }
            else
            {
                currentGroup.Add(parts[i]);
            }
        }
        groups.Add(currentGroup);

        var sentenceTexts = groups.Select(group =>
        {
            if (group.Count == 1)
                return group[0];
            // Serial comma: A, B, and C
            var allButLast = string.Join(", ", group.Take(group.Count - 1));
            return $"{allButLast}, and {group[^1]}";
        });

        var result = string.Join(". ", sentenceTexts);
        result = char.ToUpper(result[0]) + result[1..];
        if (!result.EndsWith('.') && !result.EndsWith(']'))
            result += ".";
        return result;
    }
}
