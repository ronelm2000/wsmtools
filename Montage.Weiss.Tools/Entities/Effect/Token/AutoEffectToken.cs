namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Matches auto effect (【自】) clauses and parses their labels, costs, conditions, and abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>【自】【ターン1】 ［手札を1枚置く］ あなたのCXが置かれた時、あなたはコストを払ってよい。</c></para>
/// <para><b>Regex:</c> ^【自】(?&lt;labels&gt;(?:【[^】]+】)*)\s*(?&lt;mainText&gt;.+)$</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>labels: All bracketed labels like 【ターン1】,【CXCOMBO】</description></item>
///   <item><description>mainText: All text after labels</description></item>
/// </list>
/// <para><b>Expected Full English Format:</b></para>
/// <code>[AUTO] Labels [CXCOMBO][1/TURN] [&lt;costs&gt;] &lt;During conditions&gt;, &lt;when conditions&gt;, &lt;if conditions&gt;, you may pay the cost. If you do, &lt;actions&gt;.</code>
/// <para><b>Notes:</b></para>
/// <list type="bullet">
///   <item><description>Labels are optional and can be multiple</description></item>
///   <item><description>[CXCOMBO] and [1/TURN] are labels, not costs</description></item>
///   <item><description>Do not include brackets if there are no costs</description></item>
///   <item><description>Do not include ", you may pay the cost. If you do," if there are no costs</description></item>
///   <item><description>Costs are in ［...］ format</description></item>
///   <item><description>Conditions are iteratively matched from the start of remaining text</description></item>
///   <item><description>Abilities are iteratively matched with controlled lead-in skipping</description></item>
/// </list>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different effect type indicators (【自動】)
/// - Different cost formats (ASCII brackets [...])
/// - Different label formats</para>
/// </remarks>
internal class AutoEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AutoEffectToken>();
    public override Regex Matcher => new(@"^【自】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
        var mainText = match.Groups["mainText"].Value.Trim();

        // Handle "加速 " (Accelerate) keyword prefix
        var hasAccelerate = mainText.StartsWith("加速", StringComparison.Ordinal);
        if (hasAccelerate)
        {
            mainText = Regex.Replace(mainText, @"^加速\s*", "");
        }

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

        // Translate cost using Match API
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? new List<CardEffectAbility>()
            : ParseCostText(registry, costTextJapanese);

        // Use MultiClauseEffectParser for multi-clause parsing (no sentence splitting)
        var tokenLog = new List<string>();
        var allConditions = new List<CardEffectCondition>();
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();

        // Parse ASCII-bracket condition if present
        if (!string.IsNullOrEmpty(asciiConditionJapanese))
        {
            try
            {
                var condMatchResult = registry.ConditionListRegistry.Match(asciiConditionJapanese.Trim().AsMemory());
                if (condMatchResult != null)
                {
                    tokenLog.Add(condMatchResult.Match.Token);
                    var condList = condMatchResult.Translate(registry);
                    allConditions.AddRange(condList);
                }
            }
            catch (NotImplementedException)
            {
            }
        }

        // Parse the rest as a single sentence (no 。 splitting)
        var parsed = MultiClauseEffectParser.ParseSentence(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        allConditions.AddRange(parsed.Conditions);
        foreach (var abil in parsed.Abilities)
        {
            allAbilities.Add(abil);
            abilityParts.Add(abil.AbilityText);
        }

        // Log tokens from parsed sentence
        foreach (var cond in parsed.Conditions)
        {
            tokenLog.Add($"Cond:{cond.Type}");
        }
        foreach (var abil in parsed.Abilities)
        {
            tokenLog.Add($"Abil:{abil.Prefix}");
        }

        // Format conditions
        var conditionTexts = allConditions.Select(c => c.ConditionText).Where(c => !string.IsNullOrEmpty(c)).ToList();
        for (int i = 1; i < conditionTexts.Count; i++)
        {
            if (conditionTexts[i].Length > 0)
                conditionTexts[i] = char.ToLower(conditionTexts[i][0]) + conditionTexts[i][1..];
        }
        var conditionEnglish = string.Join(", ", conditionTexts);

        // Format abilities using Prefix values
        var abilityEnglish = JoinAbilityPartsWithPrefix(allAbilities);

        // Post-process opponent references: if Japanese had 相手の, replace "your" → "your opponent's"
        // then subsequent references → "their"
        if (mainText.Contains("相手の"))
        {
            var opponentRegex = new Regex(@"(?<!\bother\s+)(?<!\byour\s+opponent['\s])\byour\b(?!\s+opponent)");
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
        var accelerateInsert = hasAccelerate ? " Accelerate" : "";
        var effectText = $"[AUTO]{accelerateInsert}{labelPrefix}";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $"{(hasAccelerate ? " " : "")}[{costEnglish}]";
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

        var finalLabels = labels;
        if (hasAccelerate)
            finalLabels = [.. finalLabels, "Accelerate"];

        return new AutoCardEffect
        {
            Labels = finalLabels,
            PreConditionText = string.Empty,
            PostConditionText = string.Empty,
            ConditionText = conditionEnglish,
            Condition = allConditions,
            CostText = costEnglish,
            Cost = costAbilities,
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText,
            TokenLog = tokenLog
        };
    }

    private static List<CardEffectAbility> ParseCostText(ITokenRegistry registry, string costText)
    {
        var costAbilities = new List<CardEffectAbility>();
        var costRemaining = costText;
        while (!string.IsNullOrWhiteSpace(costRemaining))
        {
            var t = costRemaining.TrimStart();
            var matchResult = registry.EffectListRegistry.Match(t.AsMemory());
            if (matchResult == null) break;

            var abils = matchResult.Translate(registry);
            costAbilities.AddRange(abils);
            costRemaining = t[matchResult.Match.Length..].TrimStart('、', ' ', '\t');
        }
        return costAbilities;
    }

    internal static string JoinAbilityPartsWithPrefix(List<CardEffectAbility> abilities)
    {
        if (abilities.Count == 0)
            return "";

        // Group by sentence: abilities with IfYouDo/Otherwise/AfterThat prefix start new sentences
        var groups = new List<List<CardEffectAbility>>();
        var currentGroup = new List<CardEffectAbility> { abilities[0] };
        for (int i = 1; i < abilities.Count; i++)
        {
            var abil = abilities[i];
            if (abil.Prefix is AbilityPrefix.IfYouDo or AbilityPrefix.Otherwise or AbilityPrefix.AfterThat)
            {
                groups.Add(currentGroup);
                currentGroup = new List<CardEffectAbility> { abil };
            }
            else
            {
                currentGroup.Add(abil);
            }
        }
        groups.Add(currentGroup);

        var sentenceTexts = groups.Select(group =>
        {
            if (group.Count == 1)
                return group[0].AbilityText;

            // Join using Prefix values
            var result = group[0].AbilityText;
            for (int i = 1; i < group.Count; i++)
            {
                var connector = group[i].Prefix switch
                {
                    AbilityPrefix.Continuation => ", and ",
                    AbilityPrefix.Subject => " ",
                    _ => ", ",
                };
                var nextText = group[i].AbilityText;
                if (connector == ", and " || connector == ", ")
                    nextText = char.ToLower(nextText[0]) + nextText[1..];
                result = $"{result}{connector}{nextText}";
            }
            return result;
        });

        var finalResult = string.Join(". ", sentenceTexts);
        finalResult = char.ToUpper(finalResult[0]) + finalResult[1..];
        if (!finalResult.EndsWith('.') && !finalResult.EndsWith(']') && !finalResult.EndsWith('"'))
            finalResult += ".";
        return finalResult;
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
