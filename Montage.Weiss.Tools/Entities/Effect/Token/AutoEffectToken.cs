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
        // MatchLabels now returns bracketed format (e.g. "[CXCOMBO]") for CSV comparison.
        // Strip brackets for effect text formatting — they'll be re-added by labelPrefix.
        var formatLabels = labels.Select(l => l.Trim('[', ']')).ToArray();

        // Handle keyword prefixes (加速=Accelerate, アンコール=Encore)
        var hasAccelerate = mainText.StartsWith("加速", StringComparison.Ordinal);
        if (hasAccelerate)
        {
            mainText = Regex.Replace(mainText, @"^加速\s*", "");
        }
        var hasEncore = !hasAccelerate && mainText.StartsWith("アンコール", StringComparison.Ordinal);

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

        // Use MultiClauseEffectParser.Parse for multi-sentence condition + ability parsing
        var parsedList = MultiClauseEffectParser.Parse(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var allConditions = parsedList.SelectMany(p => p.Conditions).ToList();

        // Log warnings for any sentence with unmatched remaining text
        foreach (var p in parsedList)
        {
            if (!string.IsNullOrWhiteSpace(p.Remaining) && p.Abilities.Count == 0)
            {
                Log.Debug("Parse: sentence '{Sentence}' had unparsed remaining: '{Remaining}'", p.Text, p.Remaining);
            }
        }

        conditions.AddRange(allConditions);

        // Log matched tokens
        foreach (var c in allConditions)
            tokenLog.Add($"Cond:{c.GetType().Name}");
        foreach (var a in parsedList.SelectMany(p => p.Abilities))
            tokenLog.Add($"Abil:{a.GetType().Name}");

        var conditionEnglish = conditions.AggregateToString();

        var abilityEnglish = JoinAbilityPartsFromSentences(parsedList);

        var costTexts = costAbilities.Select(a => a.AbilityText).ToList();
        var costEnglish = "";
        if (costTexts.Count > 0)
        {
            costEnglish = costTexts[0];
            for (int i = 1; i < costTexts.Count; i++)
            {
                var sep = i == 1 && Regex.IsMatch(costTexts[0], @"^\(\d+\)$") ? " " : " & ";
                costEnglish += sep + costTexts[i];
            }
            costEnglish = CapitalizeFirstAlpha(costEnglish);
        }

        var labelPrefix = formatLabels.Length > 0 ? $"[{string.Join("][", formatLabels)}]" : "";
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
            if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith('"') && !abilityForEffect.EndsWith(']'))
                effectText += ".";
        }

        var finalLabels = labels;
        if (hasAccelerate)
            finalLabels = [.. finalLabels, "Accelerate"];
        if (hasEncore)
            finalLabels = [.. finalLabels, "Encore"];

        return new AutoCardEffect
        {
            Labels = finalLabels,
            PreConditionText = string.Empty,
            PostConditionText = string.Empty,
            ConditionText = conditionEnglish,
            Condition = conditions,
            CostText = costEnglish,
            Cost = costAbilities,
            Abilities = parsedList.SelectMany(p => p.Abilities).ToList(),
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

        // Avoid double periods: trim trailing '.' before joining, re-add if needed
        var trimmed = sentenceTexts.Select(s => s.TrimEnd('.')).ToList();
        var result = string.Join(". ", trimmed);
        result = char.ToUpper(result[0]) + result[1..];
        if (!result.EndsWith('.') && !result.EndsWith(']') && !result.EndsWith('"'))
            result += ".";
        return result;
    }

    /// <summary>
    /// Joins ability lists per-sentence using <see cref="AbilityPrefix"/> for within-sentence connectors,
    /// then joins sentences with <c>". "</c>.
    /// </summary>
    /// <remarks>
    /// <para>Within each <see cref="ParsedSentence"/>, abilities are joined using prefix-specific connectors:</para>
    /// <list type="bullet">
    ///   <item><description><see cref="AbilityPrefix.And"/> (default): serial comma — <c>A, B, and C</c></description></item>
    ///   <item><description><see cref="AbilityPrefix.Continuation"/>: <c>, and</c> with redundant "this card" subject stripped</description></item>
    ///   <item><description><see cref="AbilityPrefix.Subject"/>: space separator</description></item>
    /// </list>
    /// <para>Across sentences, results are joined with <c>". "</c>. Trailing periods before <c>"</c> are preserved.</para>
    /// </remarks>
    /// <param name="sentences">Parsed sentences from <see cref="MultiClauseEffectParser.Parse"/>.</param>
    /// <returns>A single English string with all abilities combined, e.g. <c>"do X, and do Y. Then do Z."</c></returns>
    internal static string JoinAbilityPartsFromSentences(List<ParsedSentence> sentences)
    {
        var sentenceTexts = new List<string>();
        foreach (var ps in sentences)
        {
            var abilities = ps.Abilities;
            var postConditions = ps.Conditions.Where(c => c.Type == ConditionType.PostCondition).ToList();

            if (abilities.Count == 0 && postConditions.Count == 0) continue;

            string result;
            if (abilities.Count > 0)
            {
                result = abilities[0].AbilityText;
                for (int i = 1; i < abilities.Count; i++)
                {
                    var prefix = abilities[i].Prefix;
                    if (prefix == AbilityPrefix.IfYouDo)
                    {
                        result += ". If you do,";
                        continue;
                    }
                    string connector;
                    var next = abilities[i].AbilityText;
                    if (prefix == AbilityPrefix.And)
                    {
                        connector = (i == abilities.Count - 1) ? ", and " : ", ";
                    }
                    else
                    {
                        connector = prefix switch
                        {
                            AbilityPrefix.Continuation => ", and ",
                            AbilityPrefix.Subject => " ",
                            _ => ", ",
                        };
                    }
                    if (next.Length > 0 && char.IsUpper(next[0]))
                        next = char.ToLower(next[0]) + next[1..];
                    if (prefix == AbilityPrefix.Continuation)
                    {
                        if (next.StartsWith("this card ", StringComparison.Ordinal))
                            next = next[10..];
                        else if (next.StartsWith("this card's ", StringComparison.Ordinal))
                            next = next[12..];
                    }
                    result += connector + next;
                }
            }
            else
            {
                result = "";
            }

            // Append post-conditions as new sentences
            if (postConditions.Count > 0)
            {
                var postTexts = postConditions.Select(c => c.ConditionText).ToList();
                if (!string.IsNullOrEmpty(result))
                {
                    result = result.TrimEnd('.') + ". " + string.Join(". ", postTexts);
                }
                else
                {
                    result = string.Join(". ", postTexts);
                }
            }

            sentenceTexts.Add(result);
        }

        if (sentenceTexts.Count == 0) return "";

        var trimmed = sentenceTexts.Select(s => {
            if (s.Length >= 2 && s[^1] == '.' && (s[^2] == '"' || s[^2] == ']')) return s;
            return s.TrimEnd('.');
        }).ToList();
        var joined = string.Join(". ", trimmed);
        joined = char.ToUpper(joined[0]) + joined[1..];
        if (!joined.EndsWith('.') && !joined.EndsWith(']') && !joined.EndsWith('"'))
            joined += ".";
        return joined;
    }

    internal static string CapitalizeFirstAlpha(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLetter(text[i]))
            {
                var arr = text.ToCharArray();
                arr[i] = char.ToUpper(arr[i]);
                return new string(arr);
            }
        }
        return text;
    }
}
