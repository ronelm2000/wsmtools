namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class AutoEffectToken : CardTextToken<CardEffect>
{
    private static readonly string[] AbilityLeadInPrefixes =
    [
        "あなたは",
        "あなたの",
        "自分の",
        "そうしたら、",
        "そうしたら",
        "その後、",
        "その後",
        "次の",
        "そして、",
        "そして"
    ];

    public override Regex Matcher => new(@"^【自】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
        var mainText = match.Groups["mainText"].Value.Trim();

        // Expected Full English Format:
        // [AUTO] Labels [CXCOMBO][1/TURN] [<costs>] <During conditions>, <when conditions>, <if conditions>, you may pay the cost. If you do, <actions>.
        // - Labels are optional and can be multiple, e.g. [CXCOMBO][1/TURN]
        // - [CXCOMBO] is a label, not a cost, and should be included in the Labels list
        // - [1/TURN] is also a label, not a cost, and should be included in the Labels list
        // - Do not put brackets if there are no costs. For example, if the effect has no costs but has conditions and actions, it should be: [AUTO] <During conditions>, <when conditions>, <if conditions>, you may <actions>.
        // - Do not put ", you may pay the cost. If you do," if there are no costs. For example, if the effect has conditions and actions but no costs, it should be: [AUTO] <During conditions>, <when conditions>, <if conditions>, <actions>.
        // - Do not put extra spaces or commas. For example, if there are no during conditions, it should be: [AUTO] <when conditions>, <if conditions>, you may pay the cost. If you do, <actions>.

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

        // Iterative ability matching with controlled lead-in skipping.
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
                    var prefix = trimmed[..matchIndex].Trim('、', '。', ' ', '\t');
                    if (!IsIgnorableAbilityPrefix(prefix))
                        break;

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
                bool prefixSkipped = false;
                foreach (var prefix in AbilityLeadInPrefixes)
                {
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        remainingText = trimmed[prefix.Length..];
                        prefixSkipped = true;
                        break;
                    }
                }
                if (prefixSkipped) continue;

                break;
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

        // Post-process opponent references: if Japanese had 相手の, replace "your" → "your opponent's"
        // then subsequent references → "their"
        if (mainText.Contains("相手の"))
        {
            var opponentRegex = new Regex(@"(?<!\bother\s+)(?<!\byour\s+opponent's\s+)\byour\b(?!\s+opponent's)");
            abilityEnglish = opponentRegex.Replace(abilityEnglish, "your opponent's");

            var firstRef = true;
            abilityEnglish = new Regex(@"your opponent's").Replace(abilityEnglish, m =>
            {
                if (firstRef) { firstRef = false; return m.Value; }
                return "their";
            });
        }

        var costEnglish = string.Join(" & ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];

        var labelPrefix = labels.Length > 0 ? $"[{string.Join("][", labels)}]" : "";
        var effectText = $"[AUTO]{labelPrefix}";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $"[{costEnglish}]";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
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

    private static bool IsIgnorableAbilityPrefix(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        foreach (var prefix in AbilityLeadInPrefixes)
        {
            if (text.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }

        return false;
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
        if (!result.EndsWith('.') && !result.EndsWith(']') && !result.EndsWith('"'))
            result += ".";
        return result;
    }
}
