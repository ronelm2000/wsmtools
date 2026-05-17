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
    public static readonly LeadInPrefixMap DefaultPrefixMap = new(new Dictionary<string, AbilityPrefix>
    {
        { "し、", AbilityPrefix.Continuation },
        { "し", AbilityPrefix.Continuation },
        { "て、", AbilityPrefix.Continuation },
        { "て", AbilityPrefix.Continuation },
        { "そうしたら、", AbilityPrefix.IfYouDo },
        { "そうしたら", AbilityPrefix.IfYouDo },
        { "そうでないなら、", AbilityPrefix.Otherwise },
        { "そうでないなら", AbilityPrefix.Otherwise },
        { "そうでなければ、", AbilityPrefix.Otherwise },
        { "そうでなければ", AbilityPrefix.Otherwise },
        { "そうしなければ、", AbilityPrefix.Otherwise },
        { "そうしなければ", AbilityPrefix.Otherwise },
        { "その後、", AbilityPrefix.AfterThat },
        { "その後", AbilityPrefix.AfterThat },
        { "あなたは", AbilityPrefix.Subject },
        { "あなたの", AbilityPrefix.Subject },
        { "自分の", AbilityPrefix.Subject },
        { "このカードは", AbilityPrefix.Subject },
        { "このカードが", AbilityPrefix.Subject },
        { "相手の", AbilityPrefix.Subject },
        { "他のあなたの", AbilityPrefix.Subject },
        { "他の", AbilityPrefix.Subject },
        { "そして、", AbilityPrefix.Continuation },
        { "そして", AbilityPrefix.Continuation },
        { "次の", AbilityPrefix.Subject },
    });

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

        // Match abilities: try direct match first, then prefix skip if needed
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

            // Step 1: Try matching at index 0 FIRST (before any prefix stripping)
            // This allows tokens with ^あなたの, ^自分の, ^相手の etc. to match
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

            // Step 2: No match at index 0 — try skipping skippable prefixes
            bool prefixSkipped = false;
            foreach (var prefix in SkippablePrefixes)
            {
                if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                {
                    var prefixType = prefixMap?.Prefixes.TryGetValue(prefix, out var p) == true ? p : AbilityPrefix.And;
                    remainingText = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                    Log.Debug("ParseSentence: skipped prefix='{Prefix}', now trying='{Remaining}'", prefix, remainingText);

                    // Step 3: Try matching again after skipping prefix
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
                Log.Debug("ParseSentence: no prefix to skip nor fallback match, breaking. remaining='{Remaining}'", trimmed);
                break;
            }
        }

        Log.Debug("ParseSentence: done. {CondCount} conditions, {AbilCount} abilities",
            conditions.Count, abilities.Count);

        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, text);
    }

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

    public static List<ParsedSentence> Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"〔[^〕]+〕", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"コストを払ってよい。", m => m.Value.Replace("。", "\0"));
        // Protect X/Y variable definitions: Ｘは...に等しい。Ｙは...に等しい。
        protectedInput = Regex.Replace(protectedInput, @"[ＸＹXY]は[^。]*に等しい。", m => m.Value.Replace("。", "\0"));
        // Protect parenthetical notes: (CXのレベルは0として扱う), (ダメージキャンセルは発生する)
        protectedInput = Regex.Replace(protectedInput, @"（[^）]+）", m => m.Value.Replace("。", "\0"));
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
