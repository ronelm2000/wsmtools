using Montage.Weiss.Tools.Entities.Effect.Token.Ability;

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
        { "そのアタック中", " during that attack" },
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
                // Detect conjunction prefixes (e.g. その後、) in the matched text
                var detectedPrefix = AbilityPrefix.And;
                foreach (var (pattern, p) in (prefixMap ?? DefaultPrefixMap).Prefixes)
                {
                    if (p == AbilityPrefix.And || p == AbilityPrefix.Subject) continue;
                    if (trimmed.StartsWith(pattern, StringComparison.Ordinal))
                    {
                        detectedPrefix = p;
                        break;
                    }
                }
                foreach (var abil in abilList)
                {
                    var finalText = pendingDuration != null ? ApplyDuration(abil.AbilityText, pendingDuration) : abil.AbilityText;
                    Log.Debug("ParseSentence: ability '{Token}' with pending duration '{Duration}' and prefix '{Prefix}' -> '{FinalText}'",
                        abilMatch.Match.Token, pendingDuration, detectedPrefix, finalText);
                    abilities.Add(abil with { AbilityText = finalText, Prefix = detectedPrefix });
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
                            var finalText = pendingDuration != null ? ApplyDuration(abil.AbilityText, pendingDuration) : abil.AbilityText;
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
                Log.Debug("ParseSentence: no prefix to skip nor fallback match, using CatchAllAbilityToken. remaining='{Remaining}'", trimmed);
                var sentinelAbilities = new CatchAllAbilityToken().Translate(registry, trimmed.AsMemory());
                abilities.AddRange(sentinelAbilities);
                remainingText = "";
                break;
            }
        }

        // Step 4: After all abilities, try matching post-conditions (e.g. X is equal to...)
        var postConditionMatch = registry.ConditionListRegistry.Match(remainingText.TrimStart().AsMemory());
        if (postConditionMatch != null)
        {
            var postCondList = postConditionMatch.Translate(registry);
            var postConds = postCondList.Where(c => c.Type == ConditionType.PostCondition).ToList();
            if (postConds.Count > 0)
            {
                conditions.AddRange(postConds);
                remainingText = remainingText.TrimStart()[postConditionMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                Log.Debug("ParseSentence: post-condition matched, remaining='{Remaining}'", remainingText);
            }
        }

        Log.Debug("ParseSentence: done. {CondCount} conditions, {AbilCount} abilities, remaining='{Remaining}'",
            conditions.Count, abilities.Count, remainingText);

        var remaining = remainingText.Trim();
        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, text, remaining);
    }

    // Prefixes that should be skipped before matching (conjunctions + duration + あなたは ONLY)
    // IMPORTANT: Subject prefixes like 自分の, 相手の, このカードは are NOT included because
    // every ability token already encodes them in its regex. Step 1 direct match handles them.
    // EXCEPTION: あなたは must remain because tokens use あなたの (possessive の) which differs
    // from あなたは (topic は). Without stripping あなたは, token regexes with ^(?:あなたの|自分の)?
    // fail to match (は ≠ の).
    public static readonly string[] SkippablePrefixes =
    [
        // Duration prefixes (longest first)
        "次の相手のターンの終わりまで、", "次の相手のターンの終わりまで",
        "次の相手のターンの終了時まで", "次の相手のターンの終了時まで",
        "このカードのバトル中、", "このカードのバトル中",
        "あなたのターン中、", "あなたのターン中",
        "そのターン中、このカードは", "このターン中、このカードは",
        "そのアタック中、", "そのアタック中",
        "そのターン中、", "そのターン中",
        "このターン中、", "このターン中",
        // Conjunctions / Continuation particles
        "そうでないなら、", "そうでないなら",
        "そうでなければ、", "そうでなければ",
        "そうしなければ、", "そうしなければ",
        "そうしたら、", "そうしたら",
        "その後、", "その後",
        "そして、", "そして",
        "し、", "し", "て、", "て",
        // Topic marker あなたは and possessive 自分の are kept because:
        // - あなたは differs from あなたの used in token regexes (は ≠ の)
        // - 自分の is not included in all token regexes (e.g. PutTopCardToWaitingRoomToken
        //   starts with ^山札の上から, expecting 自分の to be stripped first)
        "あなたは", "自分の",
    ];

    public static List<ParsedSentence> Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"〔[^〕]+〕", m => m.Value.Replace("。", "\0"));
        // Protect `。` before `『』` — nested ability text belongs to the preceding sentence
        // Also protect `。` before `』` (inside 『』 blocks, content may have its own 。)
        protectedInput = Regex.Replace(protectedInput, @"。」|。』", m => m.Value.Replace("。", "\0"));
        protectedInput = Regex.Replace(protectedInput, @"。(?=『[^』]+』)", m => "\0");
        protectedInput = Regex.Replace(protectedInput, @"コストを払ってよい。", m => m.Value.Replace("。", "\0"));
        // Protect `。` before `そうしたら` — cascade/clause connectors (after specific patterns like コストを払ってよい)
        protectedInput = Regex.Replace(protectedInput, @"。(?=そうしたら)", m => "\0");
        // Protect X/Y variable definitions: 。Xは...に等しい。
        // First, protect the 。before X/Y definitions so they stay with their preceding ability sentence.
        // This enables PostCondition matching in ParseSentence to find them after ability consumption.
        protectedInput = Regex.Replace(protectedInput, @"。[XＸYＹ]は[^。]*に等しい", m => m.Value.Replace("。", "\0"));
        // Then protect the 。inside the definition itself (trailing 。)
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
        var mainConditions = conditions.Where(c => c.Type != ConditionType.PostCondition).ToList();
        var postConditions = conditions.Where(c => c.Type == ConditionType.PostCondition).ToList();

        var conditionPart = mainConditions.Count > 0 ? mainConditions.AggregateToString() : "";
        var abilityParts = abilities.Select(a => a.AbilityText).ToList();

        string result;
        if (!string.IsNullOrEmpty(conditionPart) && abilityParts.Count > 0)
        {
            result = $"{conditionPart}, {abilityParts[0]}";
            for (int i = 1; i < abilityParts.Count; i++)
            {
                var nextAbility = abilityParts[i];
                if (nextAbility.Length > 0 && char.IsUpper(nextAbility[0]) && nextAbility[0] != 'X')
                    nextAbility = char.ToLower(nextAbility[0]) + nextAbility[1..];
                var connector = (i == abilityParts.Count - 1) ? ", and " : ", ";
                result += connector + nextAbility;
            }
        }
        else if (!string.IsNullOrEmpty(conditionPart))
        {
            result = conditionPart;
        }
        else if (abilityParts.Count > 0)
        {
            result = string.Join(", ", abilityParts);
            result = char.ToUpper(result[0]) + result[1..];
        }
        else
        {
            result = "";
        }

        if (postConditions.Count > 0)
        {
            var postText = string.Join(". ", postConditions.Select(c => c.ConditionText));
            if (!string.IsNullOrEmpty(result))
            {
                result = result.TrimEnd('.') + ". " + postText;
            }
            else
            {
                result = postText;
            }
        }

        if (!string.IsNullOrEmpty(result) && !result.EndsWith('.') && !result.EndsWith(']') && !result.EndsWith('"'))
            result += ".";
        return result;
    }

    private static string ApplyDuration(string abilityText, string duration)
    {
        var trimmedDuration = duration.TrimStart();
        if (trimmedDuration.StartsWith("during", StringComparison.OrdinalIgnoreCase))
        {
            var dur = char.ToUpper(trimmedDuration[0]) + trimmedDuration[1..];
            var lowerAbility = abilityText.Length > 0 ? char.ToLower(abilityText[0]) + abilityText[1..] : abilityText;
            return $"{dur}, {lowerAbility}";
        }
        return abilityText + duration;
    }
}
