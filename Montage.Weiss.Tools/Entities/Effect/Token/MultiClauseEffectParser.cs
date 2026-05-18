namespace Montage.Weiss.Tools.Entities.Effect.Token;

public record LeadInPrefixMap(
    IReadOnlyDictionary<string, AbilityPrefix> Prefixes,
    IReadOnlyDictionary<string, AbilityPrefix>? Fallbacks = null);

public record ParsedSentence(
    List<CardEffectCondition> Conditions,
    List<CardEffectAbility> Abilities,
    string Text,
    string Remaining = "");

public static class MultiClauseEffectParser
{
    private static readonly Dictionary<string, string> DurationTextMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
        { "このカードのバトル中", " during this card's battle" },
        { "あなたのターン中", " during your turn" },
    };

    private static readonly HashSet<string> DurationPrefixes = new(DurationTextMap.Keys.Select(k => k + "、").Concat(DurationTextMap.Keys));

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
        string? pendingDuration = null;

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
            var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (abilMatch != null)
            {
                var abilList = abilMatch.Translate(registry);
                foreach (var abil in abilList)
                {
                    var finalText = pendingDuration != null ? abil.AbilityText + pendingDuration : abil.AbilityText;
                    Log.Debug("ParseSentence: ability '{Token}' with pending duration '{Duration}' -> '{FinalText}'",
                        abilMatch.Match.Token, pendingDuration, finalText);
                    abilities.Add(abil with { AbilityText = finalText });
                }
                pendingDuration = null;
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
                    // Check if this is a duration prefix
                    var durationKey = prefix.TrimEnd('、');
                    if (DurationTextMap.TryGetValue(durationKey, out var durText))
                    {
                        pendingDuration = durText;
                        Log.Debug("ParseSentence: detected duration prefix '{Prefix}' -> '{DurationText}'", prefix, durText);
                    }

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
                            var finalText = pendingDuration != null ? abil.AbilityText + pendingDuration : abil.AbilityText;
                            Log.Debug("ParseSentence: after prefix skip, ability '{Token}' with duration '{Duration}' -> '{FinalText}'",
                                abilMatch.Match.Token, pendingDuration, finalText);
                            abilities.Add(abil with { AbilityText = finalText, Prefix = prefixType });
                        }
                        pendingDuration = null;
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

        Log.Debug("ParseSentence: done. {CondCount} conditions, {AbilCount} abilities, remaining='{Remaining}'",
            conditions.Count, abilities.Count, remainingText);

        var remaining = remainingText.Trim();
        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, text, remaining);
    }

    // Prefixes that should be skipped before matching (conjunctions + subject prefixes + duration prefixes)
    // IMPORTANT: Longer prefixes must come before shorter ones to avoid partial matches
    public static readonly string[] SkippablePrefixes =
    [
        // Duration prefixes (longest first)
        "次の相手のターンの終わりまで、", "次の相手のターンの終わりまで",
        "次の相手のターンの終了時まで、", "次の相手のターンの終了時まで",
        "このカードのバトル中、", "このカードのバトル中",
        "あなたのターン中、", "あなたのターン中",
        "そのターン中、このカードは", "このターン中、このカードは",
        "そのターン中、", "そのターン中",
        "このターン中、", "このターン中",
        // Conjunctions
        "そうでないなら、", "そうでないなら",
        "そうでなければ、", "そうでなければ",
        "そうしなければ、", "そうしなければ",
        "そうしたら、", "そうしたら",
        "その後、", "その後",
        "そして、", "そして",
        "し、", "し", "て、", "て",
        // Subject prefixes (longer first)
        "他のあなたの", "他の",
        "このカードは", "このカードが",
        "あなたは", "あなたの", "自分の",
        "相手は", "相手の",
        "次の",
    ];

    public static List<ParsedSentence> Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"〔[^〕]+〕", m => m.Value.Replace("。", "\0"));
        // Protect `。` before `『』` — nested ability text belongs to the preceding sentence
        protectedInput = Regex.Replace(protectedInput, @"。(?=『[^』]+』)", m => "\0");
        // Protect `。` before variable definitions: パワーを＋X。Xは...に等しい。
        protectedInput = Regex.Replace(protectedInput, @"。(?=[ＸＹXY]は[^。]*に等しい)", m => "\0");
        protectedInput = Regex.Replace(protectedInput, @"コストを払ってよい。", m => m.Value.Replace("。", "\0"));
        // Protect `。` before `そうしたら` — cascade/clause connectors (after specific patterns like コストを払ってよい)
        protectedInput = Regex.Replace(protectedInput, @"。(?=そうしたら)", m => "\0");
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
