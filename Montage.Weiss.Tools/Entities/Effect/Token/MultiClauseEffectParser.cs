namespace Montage.Weiss.Tools.Entities.Effect.Token;

public record LeadInPrefixMap(
    IReadOnlyDictionary<string, AbilityPrefix> Prefixes,
    IReadOnlyDictionary<string, AbilityPrefix>? Fallbacks = null);

public record ParsedSentence(
    List<CardEffectCondition> Conditions,
    List<CardEffectAbility> Abilities,
    string Text);

public static class MultiClauseEffectParser
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(MultiClauseEffectParser));
    public static readonly LeadInPrefixMap DefaultPrefixMap = new(new Dictionary<string, AbilityPrefix>
    {
        // Continuation (し、/て、) → Continuation
        { "し、", AbilityPrefix.Continuation },
        { "し", AbilityPrefix.Continuation },
        { "て、", AbilityPrefix.Continuation },
        { "て", AbilityPrefix.Continuation },
        // If you do (そうしたら) → IfYouDo
        { "そうしたら、", AbilityPrefix.IfYouDo },
        { "そうしたら", AbilityPrefix.IfYouDo },
        // Otherwise (そうでないなら/そうでなければ/そうしなければ) → Otherwise
        { "そうでないなら、", AbilityPrefix.Otherwise },
        { "そうでないなら", AbilityPrefix.Otherwise },
        { "そうでなければ、", AbilityPrefix.Otherwise },
        { "そうでなければ", AbilityPrefix.Otherwise },
        { "そうしなければ、", AbilityPrefix.Otherwise },
        { "そうしなければ", AbilityPrefix.Otherwise },
        // After that (その後) → AfterThat
        { "その後、", AbilityPrefix.AfterThat },
        { "その後", AbilityPrefix.AfterThat },
        // Subject prefixes that should be consumed as part of ability matching (not skipped)
        { "あなたは", AbilityPrefix.Subject },
        { "あなたの", AbilityPrefix.Subject },
        { "自分の", AbilityPrefix.Subject },
        // Then/and (そして) → Continuation
        { "そして、", AbilityPrefix.Continuation },
        { "そして", AbilityPrefix.Continuation },
        // These are part of ability text, NOT lead-in prefixes to skip
        // "相手の", "他の", "他のあなたの", "このカードは", "このカードが", "次の" — removed from skip list
    });

    // Prefixes that should be skipped before matching (conjunctions only)
    public static readonly string[] SkippablePrefixes =
    [
        "し、", "し", "て、", "て",
        "そうしたら、", "そうしたら",
        "そうでないなら、", "そうでないなら",
        "そうでなければ、", "そうでなければ",
        "そうしなければ、", "そうしなければ",
        "その後、", "その後",
        "そして、", "そして",
    ];

    public static (AbilityPrefix Prefix, string Remaining) DetectLeadInPrefix(string input, LeadInPrefixMap? map = null)
    {
        map ??= DefaultPrefixMap;
        foreach (var (pattern, prefix) in map.Prefixes)
        {
            if (input.StartsWith(pattern, StringComparison.Ordinal))
                return (prefix, input[pattern.Length..].TrimStart('、', ' ', '\t'));
        }
        return (AbilityPrefix.And, input);
    }

    public static ParsedSentence ParseSentence(
        string sentence,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        var conditions = new List<CardEffectCondition>();
        var abilities = new List<CardEffectAbility>();
        var remainingText = sentence.Trim();

        Log.Debug("ParseSentence: input='{Input}'", sentence);

        // Match conditions from start
        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0) break;

            var condMatch = registry.ConditionListRegistry.Match(trimmed.AsMemory());
            if (condMatch != null)
            {
                var condList = condMatch.Translate(registry);
                conditions.AddRange(condList);
                Log.Debug("ParseSentence: condition matched by '{Token}', consumed {Len} chars, remaining='{Remaining}'",
                    condMatch.Match.Token, condMatch.Match.Length, trimmed[condMatch.Match.Length..]);
                remainingText = trimmed[condMatch.Match.Length..].TrimStart('、', ' ', '\t');
            }
            else
            {
                Log.Debug("ParseSentence: no more conditions, remaining='{Remaining}'", trimmed);
                break;
            }
        }

        // Match abilities with prefix skipping
        int iteration = 0;
        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            iteration++;
            if (iteration > 20)
            {
                Log.Debug("ParseSentence: too many iterations, breaking. remaining='{Remaining}'", remainingText);
                break;
            }
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0) break;

            Log.Debug("ParseSentence: ability iteration {Iter}, trying to match='{Text}'", iteration, trimmed);

            // Try matching at index 0 first
            var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (abilMatch != null)
            {
                var abilList = abilMatch.Translate(registry);
                foreach (var abil in abilList)
                {
                    abilities.Add(abil);
                }
                Log.Debug("ParseSentence: ability matched by '{Token}', consumed {Len} chars, remaining='{Remaining}'",
                    abilMatch.Match.Token, abilMatch.Match.Length, trimmed[abilMatch.Match.Length..]);
                remainingText = trimmed[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                continue;
            }

            Log.Debug("ParseSentence: no direct match, trying skippable prefixes...");

            // No match at index 0 — try skipping skippable prefixes
            bool prefixSkipped = false;
            foreach (var prefix in SkippablePrefixes)
            {
                if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                {
                    var prefixType = prefixMap?.Prefixes.TryGetValue(prefix, out var p) == true ? p : AbilityPrefix.And;
                    remainingText = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                    Log.Debug("ParseSentence: skipped prefix='{Prefix}', now trying='{Remaining}'", prefix, remainingText);

                    // Try matching again after skipping
                    abilMatch = registry.EffectListRegistry.Match(remainingText.AsMemory());
                    if (abilMatch != null)
                    {
                        var abilList = abilMatch.Translate(registry);
                        foreach (var abil in abilList)
                        {
                            abilities.Add(abil with { Prefix = prefixType });
                        }
                        Log.Debug("ParseSentence: after prefix skip, matched by '{Token}', remaining='{Remaining}'",
                            abilMatch.Match.Token, remainingText[abilMatch.Match.Length..]);
                        remainingText = remainingText[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                        prefixSkipped = true;
                        break;
                    }

                    // Still no match — skip prefix and continue outer loop
                    prefixSkipped = true;
                    break;
                }
            }

            if (!prefixSkipped)
            {
                // Try skipping subject prefixes that are rarely part of token regexes.
                // NOTE: 'あなたの', '相手の', '他の', 'このカードは', 'このカードが', '次の' are EXCLUDED
                // because many token regexes include them (e.g. AllCharactersBoostToken: ^あなたのキャラすべてに).
                foreach (var prefix in new[] { "あなたは", "自分の" })
                {
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        remainingText = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                        Log.Debug("ParseSentence: skipped subject prefix='{Prefix}', now='{Remaining}'", prefix, remainingText);
                        prefixSkipped = true;
                        break;
                    }
                }
            }

            // Fallback: try matching the original (un-skipped) text.
            // This handles tokens whose regex includes the prefix (e.g. GiveEncoreToOpponentCharactersToken starts with "相手の")
            if (!prefixSkipped)
            {
                var originalMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
                if (originalMatch != null)
                {
                    var abilList = originalMatch.Translate(registry);
                    foreach (var abil in abilList)
                    {
                        abilities.Add(abil);
                    }
                    Log.Debug("ParseSentence: fallback match by '{Token}', consumed {Len} chars, remaining='{Remaining}'",
                        originalMatch.Match.Token, originalMatch.Match.Length, trimmed[originalMatch.Match.Length..]);
                    remainingText = trimmed[originalMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                    continue;
                }

                Log.Debug("ParseSentence: no prefix to skip nor fallback match, breaking. remaining='{Remaining}'", trimmed);
                break;
            }
        }

        Log.Debug("ParseSentence: done. {CondCount} conditions, {AbilCount} abilities",
            conditions.Count, abilities.Count);

        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, text);
    }

    public static List<ParsedSentence> Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        // Protect 「」, 『』, and cost-pay patterns from 。 splitting
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"〔[^〕]+〕", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"コストを払ってよい。", m => m.Value.Replace("。", "\0"));
        var sentences = protectedInput.Split('。', StringSplitOptions.RemoveEmptyEntries);
        var results = new List<ParsedSentence>();

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim().Replace("\0", "。");
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var parsed = ParseSentence(trimmed, registry, prefixMap);
            results.Add(parsed);
        }

        return results;
    }

    private static string BuildSentenceText(List<CardEffectCondition> conditions, List<CardEffectAbility> abilities)
    {
        var parts = new List<string>();
        parts.AddRange(conditions.Select(c => c.ConditionText));
        parts.AddRange(abilities.Select(a => a.AbilityText));

        if (parts.Count == 0) return "";

        var result = string.Join(", ", parts);
        result = char.ToUpper(result[0]) + result[1..];
        if (!result.EndsWith('.') && !result.EndsWith(']') && !result.EndsWith('"'))
            result += ".";
        return result;
    }
}
