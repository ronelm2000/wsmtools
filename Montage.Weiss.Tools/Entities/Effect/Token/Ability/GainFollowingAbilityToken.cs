namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostWithDurationToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PowerBoostWithDurationToken>();

    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?<power>\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        Log.Debug("PowerBoostWithDurationToken: matched input='{Input}', power={Power}, duration='{Duration}'",
            span.ToString(), power, durationText);

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power{durationText}"
            }
        ];
    }
}

/// <summary>
/// Matches "power boost and gain following ability" clauses.
/// Emits Oxford comma before "and the following ability" and ensures the nested English text ends with a period.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのパワーを＋4500し、このカードは次の能力を得る。『…』</c></para>
/// <para><b>Regex:</b> ^(?:(?&lt;duration&gt;そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?&lt;power&gt;\d+)し、このカードは次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "duration": Optional duration prefix</description></item>
///   <item><description>Group "power": Power boost amount (e.g., "4500")</description></item>
///   <item><description>Group "nested": Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets +{power} power, and the following ability{duration}. "{nestedEnglish}"</c></para>
/// </remarks>
internal class PowerBoostWithFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?<power>\d+)し、このカードは次の能力を得る。『(?<nested>.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;
        var nestedJapanese = match.Groups["nested"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        var nestedEnglish = TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        if (!nestedEnglish.EndsWith('.') && !nestedEnglish.EndsWith('"'))
            nestedEnglish += ".";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power, and the following ability{durationText}. \"{nestedEnglish}\""
            }
        ];
    }

    private static readonly string[] NestedLeadInPrefixes =
    [
        "あなたは",
        "自分の",
        "そうしたら、",
        "そうしたら",
        "その後、",
        "その後",
        "そして、",
        "そして"
    ];

    internal static string? TryTranslateNested(ITokenRegistry registry, string japanese)
    {
        var trimmed = japanese.Trim();
        if (trimmed.Length == 0)
            return null;

        // Strip lead-in prefixes
        foreach (var prefix in NestedLeadInPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
            {
                var rest = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                if (!string.IsNullOrEmpty(rest) && rest != trimmed)
                    return TryTranslateNested(registry, rest);
            }
        }

        // Strip 【】 prefix first and try ability match on the stripped content
        var prefixMatch = Regex.Match(trimmed, @"^【(自|永|起|カウンター)】\s*");
        if (prefixMatch.Success)
        {
            var effectType = prefixMatch.Groups[1].Value;
            var prefixStripped = trimmed[prefixMatch.Length..];
            var strippedResult = TryTranslateNested(registry, prefixStripped);
            if (strippedResult != null)
            {
                var space = strippedResult.StartsWith('[') ? "" : " ";
                var typePrefix = effectType switch
                {
                    "自" => "[AUTO]",
                    "永" => "[CONT]",
                    "起" => "[ACT]",
                    "カウンター" => "[COUNTER]",
                    _ => ""
                };
                var sep = strippedResult.StartsWith("[") ? "" : " ";
                return typePrefix + sep + strippedResult;
            }
        }

        // Handle cost brackets ［...］ prefix (e.g. ［(1)］)
        var costBracketMatch = Regex.Match(trimmed, @"^［(.+?)］\s*(.*)$");
        if (costBracketMatch.Success)
        {
            var costContent = costBracketMatch.Groups[1].Value;
            var restAfterCost = costBracketMatch.Groups[2].Value;
            var costResult = TryTranslateNested(registry, costContent);
            var restResult = TryTranslateNested(registry, restAfterCost);
            if (costResult != null || restResult != null)
            {
                var costPart = costResult ?? costContent;
                var restPart = restResult ?? "";
                var separator = costPart.StartsWith("[") ? "" : " ";
                return $"[{costPart}]{separator}{restPart}".TrimEnd();
            }
        }

        // Try condition+ability parsing via ParseSentence first (handles conditions before abilities)
        var multiParsed = MultiClauseEffectParser.Parse(trimmed, registry, MultiClauseEffectParser.DefaultPrefixMap);
        if (multiParsed.Count > 1)
        {
            var parts = new List<string>();
            foreach (var p in multiParsed)
            {
                if (p.Abilities.Count > 0 || p.Conditions.Count > 0)
                    parts.Add(p.Text);
            }
            if (parts.Count > 0)
            {
                var joined = string.Join(". ", parts.Select(s => s.TrimEnd('.')));
                joined = char.ToUpper(joined[0]) + joined[1..];
                if (!joined.EndsWith('.')) joined += ".";
                return joined;
            }
        }

        var parsed = MultiClauseEffectParser.ParseSentence(trimmed, registry, MultiClauseEffectParser.DefaultPrefixMap);
        if (parsed.Abilities.Count > 0 || parsed.Conditions.Count > 0)
        {
            if (parsed.Conditions.Count > 0 && parsed.Abilities.Count == 0)
            {
                // Conditions-only with no abilities means this is a continuous (CONT) effect
                // matched as a condition. Return the condition text directly without "If" prefix.
                return string.Join(", ", parsed.Conditions.Select(c => c.ConditionText));
            }
            if (parsed.Conditions.Count > 0)
                return parsed.Text;
            if (parsed.Abilities.Count == 1)
                return parsed.Abilities[0].AbilityText;
            return AutoEffectToken.JoinAbilityPartsFromSentences([parsed]);
        }

        // Try ability match (index-0 via Match API) — for pure-ability text with no conditions
        if (TryMatchAbility(registry, trimmed, out var abilityResult))
            return abilityResult;

        // Try effect match as last resort (may match EventEffectToken which is lossy for nested contexts)
        if (TryMatchEffect(registry, trimmed, out var effectResult))
            return effectResult;

        var blockSplit = Regex.Split(trimmed, @"』\s*『");
        if (blockSplit.Length > 1)
        {
            var parts = blockSplit.Select(b =>
            {
                var cleaned = b.Trim();
                if (!cleaned.StartsWith("『")) cleaned = "『" + cleaned;
                if (!cleaned.EndsWith("』")) cleaned = cleaned + "』";
                var restripped = Regex.Replace(cleaned, @"^『(.+)』$", "$1");
                return TryTranslateNested(registry, restripped) ?? restripped;
            }).Where(p => p != null).ToList();
            if (parts.Count > 0)
                return string.Join(" ", parts);
        }

        var sentences = Regex.Split(trimmed, @"(?<=。)");
        if (sentences.Length > 1)
        {
            var translatedSentences = new List<string>();
            foreach (var sentence in sentences)
            {
                var s = sentence.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var sentenceResult = TryTranslateNested(registry, s);
                translatedSentences.Add(sentenceResult ?? s);
            }
            if (translatedSentences.Count > 0)
                return string.Join(" ", translatedSentences);
        }

        return null;
    }

    private static bool TryMatchAbility(ITokenRegistry registry, string japanese, out string? result)
    {
        // Only try index-0 match. Prefix skipping with duration tracking is handled by ParseSentence.
        var matchResult = registry.EffectListRegistry.Match(japanese.TrimStart().AsMemory());
        if (matchResult != null)
        {
            var abils = matchResult.Translate(registry);
            result = string.Join(", ", abils.Select(a => a.AbilityText));
            var remaining = japanese.TrimStart()[matchResult.Match.Length..].TrimStart('、', '。', ' ', '\t');
            if (!string.IsNullOrEmpty(remaining))
            {
                var restResult = TryTranslateNested(registry, remaining);
                if (restResult != null)
                {
                    var separator = result.EndsWith('.') ? " " : ". ";
                    result += separator + restResult;
                }
            }
            return true;
        }
        result = null;
        return false;
    }

    private static bool TryMatchEffect(ITokenRegistry registry, string japanese, out string? result)
    {
        var matchResult = registry.EffectRegistry.Match(japanese.AsMemory());
        if (matchResult != null)
        {
            var effect = matchResult.Translate(registry);
            result = effect.EffectText;
            var remaining = japanese[matchResult.Match.Length..].TrimStart('、', '。', ' ', '\t');
            if (!string.IsNullOrEmpty(remaining))
            {
                var restResult = TryTranslateNested(registry, remaining);
                if (restResult != null)
                    result += " " + restResult;
            }
            return true;
        }
        result = null;
        return false;
    }
}

internal class PowerBoostWithFollowingAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", "until end of turn" },
        { "このターン中", "until end of turn" },
        { "次の相手のターンの終わりまで", "until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", "until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを＋(?<power>\d+)し、このカードは次の2つの能力を得る。『(?<nested1>.+)』『(?<nested2>.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;
        var nestedJapanese1 = match.Groups["nested1"].Value;
        var nestedJapanese2 = match.Groups["nested2"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = $" {dur}";
        }

        var nestedEnglish1 = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese1) ?? nestedJapanese1;
        var nestedEnglish2 = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese2) ?? nestedJapanese2;
        if (!nestedEnglish1.EndsWith('.') && !nestedEnglish1.EndsWith('"') && !nestedEnglish1.EndsWith(']') && nestedEnglish1.Contains(' '))
            nestedEnglish1 += ".";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and the following abilities{durationText}. \"{nestedEnglish1}\" \"{nestedEnglish2}\""
            }
        ];
    }
}

internal class GainFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^し、このカードが次の能力を得る(?:。『(.+)』)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Success ? match.Groups[1].Value : null;
        var nestedEnglish = nestedJapanese != null ? PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) : null;
        var abilityText = "get the following ability";
        if (nestedEnglish != null)
            abilityText += $". \"{nestedEnglish}\"";
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}

internal class GainFollowingAbilityTokenWithParticleWa : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^し、このカードは次の能力を得る(?:。『(.+)』)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Success ? match.Groups[1].Value : null;
        var nestedEnglish = nestedJapanese != null ? PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) : null;
        var abilityText = "get the following ability";
        if (nestedEnglish != null)
            abilityText += $". \"{nestedEnglish}\"";
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}

/// <summary>
/// Matches standalone "<c>次の能力を得る。『...』</c>" clauses (no leading <c>し、</c>).
/// Used after prefixes like <c>そのターン中、このカードは</c> have been stripped.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の能力を得る。『【永】 あなたの[[shot.gif]]の効果で与えるダメージを＋1。』</c></para>
/// <para><b>Regex:</b> ^次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>get the following ability. "[CONT] ..."</c></para>
/// </remarks>
internal class GainStandaloneFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の能力を得る。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"get the following ability. \"{nestedEnglish}\""
            }
        ];
    }
}

/// <summary>
/// Matches "<c>(その後、)?あなたのキャラすべてに...次の能力を与える。『...』</c>" clauses.
/// Used for granting abilities to all characters, optionally after "その後、".
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>その後、あなたのキャラすべてに、そのターン中、次の能力を与える。『【自】 このカードがアタックした時、...』</c></para>
/// <para><b>Regex:</b> ^(?:その後、)?あなたのキャラすべてに、そのターン中、次の能力を与える。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>all of your characters get the following ability until end of turn. "[AUTO] ..."</c></para>
/// </remarks>
internal class AfterThatAllCharactersGetAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:その後、)?あなたのキャラすべてに、そのターン中、次の能力を与える。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your characters get the following ability until end of turn. \"{nestedEnglish}\""
            }
        ];
    }
}

/// <summary>
/// Matches "<c>(そのターン中、)?(?:このカードは)?次の能力を得る。『...』</c>" clauses.
/// Variant that captures an optional duration prefix before "次の能力を得る".
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのターン中、このカードは次の能力を得る。『【自】 ...』</c></para>
/// <para><b>Regex:</b> ^(?:そのターン中、)?(?:このカードは)?次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets the following ability until end of turn. "[AUTO] ..."</c> (with duration) / <c>get the following ability. "[AUTO] ..."</c> (without duration)</para>
/// </remarks>
internal class GainFollowingAbilityWithDurationToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:そのターン中、)?(?:このカードは)?次の能力を得る。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;
        var hasDuration = match.Value.Contains("そのターン中");
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasDuration
                    ? $"this card gets the following ability until end of turn. \"{nestedEnglish}\""
                    : $"get the following ability. \"{nestedEnglish}\""
            }
        ];
    }
}



