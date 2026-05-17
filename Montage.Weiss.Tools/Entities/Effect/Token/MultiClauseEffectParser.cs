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

        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0) break;

            var condMatch = registry.ConditionListRegistry.Match(trimmed.AsMemory());
            if (condMatch != null)
            {
                var condList = condMatch.Translate(registry);
                conditions.AddRange(condList);
                remainingText = trimmed[condMatch.Match.Length..].TrimStart('、', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0) break;

            var (prefix, remaining) = DetectLeadInPrefix(trimmed, prefixMap);
            if (prefix != AbilityPrefix.And)
            {
                remainingText = remaining;
                continue;
            }

            var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (abilMatch != null)
            {
                var abilList = abilMatch.Translate(registry);
                foreach (var abil in abilList)
                {
                    abilities.Add(abil with { Prefix = prefix });
                }
                remainingText = trimmed[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        var text = BuildSentenceText(conditions, abilities);
        return new ParsedSentence(conditions, abilities, text);
    }

    public static List<ParsedSentence> Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null)
    {
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
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
