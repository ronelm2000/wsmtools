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
        var costAbilities = new List<CardEffectAbility>();
        var tokenLog = new List<string>();
        if (!string.IsNullOrEmpty(costTextJapanese))
        {
            var costRemaining = costTextJapanese;
            while (!string.IsNullOrWhiteSpace(costRemaining))
            {
                var t = costRemaining.TrimStart();
                var m = registry.EffectListRegistry.Match(t.AsMemory());
                if (m == null) break;
                var abils = m.Translate(registry);
                costAbilities.AddRange(abils);
                tokenLog.Add($"Cost:{m.Match.Token}");
                costRemaining = t[m.Match.Length..].TrimStart('、', ' ', '\t');
            }
        }

        // Parse ASCII-bracket condition if present
        var conditions = new List<CardEffectCondition>();
        if (!string.IsNullOrEmpty(asciiConditionJapanese))
        {
            try
            {
                var condMatch = registry.ConditionListRegistry.Match(asciiConditionJapanese.Trim().AsMemory());
                if (condMatch != null)
                {
                    tokenLog.Add($"Cond:{condMatch.Match.Token}");
                    var condList = condMatch.Translate(registry);
                    conditions.AddRange(condList);
                }
            }
            catch (NotImplementedException)
            {
            }
        }

        // Use MultiClauseEffectParser.ParseSentence for condition + ability parsing
        var parsed = MultiClauseEffectParser.ParseSentence(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        conditions.AddRange(parsed.Conditions);
        var allAbilities = parsed.Abilities;

        // Log matched tokens
        foreach (var c in parsed.Conditions)
            tokenLog.Add($"Cond:{c.GetType().Name}");
        foreach (var a in allAbilities)
            tokenLog.Add($"Abil:{a.GetType().Name}");

        var abilityParts = allAbilities.Select(a => a.AbilityText).ToList();

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
            if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith(']') && !abilityForEffect.EndsWith('"'))
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
            Condition = conditions,
            CostText = costEnglish,
            Cost = costAbilities,
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText,
            TokenLog = tokenLog
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
        if (!result.EndsWith('.') && !result.EndsWith(']') && !result.EndsWith('"'))
            result += ".";
        return result;
    }
}
