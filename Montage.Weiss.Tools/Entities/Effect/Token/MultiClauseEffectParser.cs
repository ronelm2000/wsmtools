using Montage.Weiss.Tools.Entities.Effect.Token.Ability;

namespace Montage.Weiss.Tools.Entities.Effect.Token;

public record LeadInPrefixMap(
    IReadOnlyDictionary<string, AbilityPrefix> Prefixes,
    IReadOnlyDictionary<string, AbilityPrefix>? Fallbacks = null);

public record ParsedSentence(
    List<CardEffectCondition> Conditions,
    List<CardEffectAbility> Abilities,
    List<string> ConditionTokenNames,
    List<string> AbilityTokenNames,
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
        { "ないなら、", AbilityPrefix.Otherwise },
        { "ないなら", AbilityPrefix.Otherwise },
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
        var conditionTokenNames = new List<string>();
        var abilityTokenNames = new List<string>();
        var remainingText = sentence.Trim();
        string? pendingDuration = null;
        AbilityPrefix? pendingPrefix = null;

        Log.Debug("ParseSentence: input='{Input}'", sentence);

        // Match conditions from start
        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0) break;

            // Before matching conditions, check for Otherwise/AfterThat prefixes
            // that should break into the ability loop instead of being consumed as conditions.
            var activeMap = prefixMap ?? DefaultPrefixMap;
            bool isLeadInPrefix = activeMap.Prefixes.Any(p =>
                p.Value is AbilityPrefix.Otherwise or AbilityPrefix.AfterThat &&
                trimmed.StartsWith(p.Key, StringComparison.Ordinal));
            if (isLeadInPrefix)
            {
                Log.Debug("ParseSentence: detected lead-in prefix, breaking to ability loop. trimmed='{Trimmed}'", trimmed);
                break;
            }

            var condMatch = registry.ConditionListRegistry.Match(trimmed.AsMemory());
            if (condMatch != null)
            {
                // If CatchAllConditionToken would consume text containing ないなら as part of
                // a continuation chain (し、ないなら), break — it's an Otherwise clause, not a condition.
                if (condMatch.Match.Token == nameof(Condition.CatchAllConditionToken) &&
                    trimmed.Length >= condMatch.Match.Length &&
                    (trimmed[..condMatch.Match.Length].Contains("し、ないなら") ||
                     trimmed[..condMatch.Match.Length].Contains("選び、") ||
                     trimmed[..condMatch.Match.Length].Contains("マーカーすべてを、") ||
                     trimmed[..condMatch.Match.Length].Contains("他のあなたのカード名に") ||
                     Regex.IsMatch(trimmed[..condMatch.Match.Length], @"カード名.*に「.+」を含むキャラがいるなら")))  // let ConditionalAbilityToken handle this
                {
                    Log.Debug("ParseSentence: CatchAllConditionToken matched compound ability pattern, breaking to ability loop. trimmed='{Trimmed}'", trimmed);
                    break;
                }

                var condList = condMatch.Translate(registry);
                conditions.AddRange(condList);
                conditionTokenNames.Add(condMatch.Match.Token);
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
                var detectedPrefix = pendingPrefix ?? AbilityPrefix.And;
                if (pendingPrefix == null)
                {
                    foreach (var (pattern, p) in (prefixMap ?? DefaultPrefixMap).Prefixes)
                    {
                        if (p == AbilityPrefix.And || p == AbilityPrefix.Subject) continue;
                        if (trimmed.StartsWith(pattern, StringComparison.Ordinal))
                        {
                            detectedPrefix = p;
                            break;
                        }
                    }
                }
                abilityTokenNames.Add(abilMatch.Match.Token);
                foreach (var abil in abilList)
                {
                    var prefix = abil.Prefix != AbilityPrefix.And ? abil.Prefix : detectedPrefix;
                    if (pendingDuration != null)
                    {
                        var finalText = ApplyDuration(abil.AbilityText, pendingDuration);
                        Log.Debug("ParseSentence: ability '{Token}' with pending duration '{Duration}' and prefix '{Prefix}' -> '{FinalText}'",
                            abilMatch.Match.Token, pendingDuration, prefix, finalText);
                        abilities.Add(abil with { AbilityText = finalText, Prefix = prefix });
                    }
                    else
                    {
                        Log.Debug("ParseSentence: ability '{Token}' with prefix '{Prefix}' -> '{FinalText}'",
                            abilMatch.Match.Token, prefix, abil.AbilityText);
                        abilities.Add(abil with { Prefix = prefix });
                    }
                }
                pendingDuration = null;
                pendingPrefix = null;
                Log.Debug("ParseSentence: ability matched by '{Token}', consumed {Len} chars, remaining='{Remaining}'",
                    abilMatch.Match.Token, abilMatch.Match.Length, trimmed[abilMatch.Match.Length..]);
                remainingText = trimmed[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t', '』');
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
                        abilityTokenNames.Add(abilMatch.Match.Token);
                        foreach (var abil in abilList)
                        {
                            var finalText = pendingDuration != null ? ApplyDuration(abil.AbilityText, pendingDuration) : abil.AbilityText;
                            var effectivePrefix = pendingPrefix ?? prefixType;
                            Log.Debug("ParseSentence: after prefix skip, ability '{Token}' with duration '{Duration}' and prefix '{Prefix}' -> '{FinalText}'",
                                abilMatch.Match.Token, pendingDuration, effectivePrefix, finalText);
                            abilities.Add(abil with { AbilityText = finalText, Prefix = effectivePrefix });
                        }
                        pendingDuration = null;
                        pendingPrefix = null;
                        Log.Debug("ParseSentence: after prefix skip, matched by '{Token}', remaining='{Remaining}'",
                            abilMatch.Match.Token, remainingText[abilMatch.Match.Length..]);
                        remainingText = remainingText[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t', '』');
                        prefixSkipped = true;
                        break;
                    }

                    // Still no match — save non-duration prefix for later use, skip prefix and continue outer loop
                    if (pendingDuration == null && prefixType is AbilityPrefix.Otherwise or AbilityPrefix.AfterThat)
                    {
                        pendingPrefix = prefixType;
                    }
                    prefixSkipped = true;
                    break;
                }
            }

            if (!prefixSkipped)
            {
                // Check for post-conditions (e.g. X is equal to...) before falling through to CatchAllAbilityToken
                var postCheck = registry.ConditionListRegistry.Match(trimmed.AsMemory());
                if (postCheck != null)
                {
                    var postCondList = postCheck.Translate(registry);
                    var postConds = postCondList.Where(c => c.Type == ConditionType.PostCondition).ToList();
                    if (postConds.Count > 0)
                    {
                        conditions.AddRange(postConds);
                        conditionTokenNames.Add(postCheck.Match.Token);
                        remainingText = trimmed[postCheck.Match.Length..].TrimStart('、', '。', ' ', '\t');
                        Log.Debug("ParseSentence: post-condition matched in ability loop, remaining='{Remaining}'", remainingText);
                        break;
                    }
                }
                Log.Debug("ParseSentence: no prefix to skip nor fallback match, using CatchAllAbilityToken. remaining='{Remaining}'", trimmed);
                var sentinelAbilities = new CatchAllAbilityToken().Translate(registry, trimmed.AsMemory());
                abilities.AddRange(sentinelAbilities);
                abilityTokenNames.Add(nameof(CatchAllAbilityToken));
                remainingText = "";
                break;
            }
        }

        // Step 4: After all abilities, try matching post-conditions (e.g. X is equal to...)
        var remainingTrimmed = remainingText.TrimStart();
        TokenMatchResult<List<CardEffectCondition>>? postConditionMatch = null;
        if (remainingTrimmed.Length > 0)
            postConditionMatch = registry.ConditionListRegistry.Match(remainingTrimmed.AsMemory());
        if (postConditionMatch != null)
        {
            var postCondList = postConditionMatch.Translate(registry);
            var postConds = postCondList.Where(c => c.Type == ConditionType.PostCondition).ToList();
            if (postConds.Count > 0)
            {
                conditions.AddRange(postConds);
                conditionTokenNames.Add(postConditionMatch.Match.Token);
                remainingText = remainingText.TrimStart()[postConditionMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                Log.Debug("ParseSentence: post-condition matched, remaining='{Remaining}'", remainingText);
            }
        }

        Log.Debug("ParseSentence: done. {CondCount} conditions, {AbilCount} abilities, remaining='{Remaining}'",
            conditions.Count, abilities.Count, remainingText);

        var remaining = remainingText.Trim();
        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, conditionTokenNames, abilityTokenNames, text, remaining);
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
        "ないなら、", "ないなら",
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

        string result;
        if (!string.IsNullOrEmpty(conditionPart) && abilities.Count > 0)
        {
            result = $"{conditionPart}, {abilities[0].AbilityText}";
            for (int i = 1; i < abilities.Count; i++)
            {
                var nextAbility = abilities[i].AbilityText;
                if (nextAbility.Length > 0 && char.IsUpper(nextAbility[0]) && nextAbility[0] != 'X')
                    nextAbility = char.ToLower(nextAbility[0]) + nextAbility[1..];
                string connector;
                if (abilities[i].Prefix == AbilityPrefix.AfterCannotBePlayed)
                {
                    connector = ". ";
                    nextAbility = char.ToUpper(nextAbility[0]) + nextAbility[1..];
                }
                else
                {
                    connector = (i == abilities.Count - 1 && abilities[i].Prefix != AbilityPrefix.Continuation) ? ", and " : ", ";
                    if (abilities[i - 1].Prefix == AbilityPrefix.Continuation)
                        connector = ", and ";
                }
                result += connector + nextAbility;
            }
        }
        else if (!string.IsNullOrEmpty(conditionPart))
        {
            result = conditionPart;
        }
        else if (abilities.Count > 0)
        {
            result = abilities[0].AbilityText;
            for (int i = 1; i < abilities.Count; i++)
            {
                var nextAbility = abilities[i].AbilityText;
                if (nextAbility.Length > 0 && char.IsUpper(nextAbility[0]) && nextAbility[0] != 'X')
                    nextAbility = char.ToLower(nextAbility[0]) + nextAbility[1..];
                string connector;
                if (abilities[i].Prefix == AbilityPrefix.AfterCannotBePlayed)
                {
                    connector = ". ";
                    nextAbility = char.ToUpper(nextAbility[0]) + nextAbility[1..];
                }
                else
                {
                    connector = (i == abilities.Count - 1 && abilities[i].Prefix != AbilityPrefix.Continuation) ? ", and " : ", ";
                    if (abilities[i - 1].Prefix == AbilityPrefix.Continuation)
                        connector = ", and ";
                }
                result += connector + nextAbility;
            }
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
                if (result.EndsWith(".\""))
                    result = result + " " + postText;
                else
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
