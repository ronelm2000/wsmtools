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

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?<power>\d+)");

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

internal class PowerBoostWithFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのパワーを[＋\+](?<power>\d+)し、このカードは次の能力を得る。『(?<nested>.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups["power"].Value;
        var nestedJapanese = match.Groups["nested"].Value;

        var nestedEnglish = TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and the following ability. \"{nestedEnglish}\""
            }
        ];
    }

    internal static string? TryTranslateNested(ITokenRegistry registry, string japanese)
    {
        if (TryMatchAny(registry, japanese, out var result))
            return result;

        var trimmed = japanese.Trim();
        if (trimmed.Length == 0)
            return null;

        var parsed = MultiClauseEffectParser.ParseSentence(trimmed, registry);
        if (parsed.Abilities.Count > 0 || parsed.Conditions.Count > 0)
        {
            return parsed.Text;
        }

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

        var prefixStripped = Regex.Replace(trimmed, @"^【(自|永|起|カウンター)】\s*", "");
        if (prefixStripped != trimmed)
        {
            var strippedResult = TryTranslateNested(registry, prefixStripped);
            if (strippedResult != null)
                return strippedResult;
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

    private static readonly string[] NestedLeadInPrefixes =
    [
        "あなたは",
        "あなたの",
        "自分の",
        "そうしたら、",
        "そうしたら",
        "その後、",
        "その後",
        "そして、",
        "そして"
    ];

    private static bool TryMatchAny(ITokenRegistry registry, string japanese, out string? result)
    {
        if (registry.EffectRegistry.TryFindFirstMatch(japanese, out var effectFunc, out _, out var consumed) && effectFunc != null)
        {
            var effect = effectFunc(registry);
            result = effect.EffectText;
            var remaining = japanese[consumed..].TrimStart();
            if (!string.IsNullOrEmpty(remaining))
            {
                var restResult = TryTranslateNested(registry, remaining);
                if (restResult != null)
                    result += " " + restResult;
            }
            return true;
        }

        if (registry.EffectListRegistry.TryFindFirstMatch(japanese, out var abilFunc, out _, out var abilConsumed) && abilFunc != null)
        {
            var abils = abilFunc(registry);
            result = string.Join(", ", abils.Select(a => a.AbilityText));
            var remaining = japanese[abilConsumed..].TrimStart();
            if (!string.IsNullOrEmpty(remaining))
            {
                var restResult = TryTranslateNested(registry, remaining);
                if (restResult != null)
                    result += " " + restResult;
            }
            return true;
        }

        // Strip lead-in prefixes and retry
        var trimmed = japanese.TrimStart();
        foreach (var prefix in NestedLeadInPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
            {
                var rest = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                if (!string.IsNullOrEmpty(rest) && rest != japanese)
                    return TryMatchAny(registry, rest, out result);
            }
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



