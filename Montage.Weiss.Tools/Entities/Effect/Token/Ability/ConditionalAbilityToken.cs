namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches conditional ability patterns where a condition precedes an ability action.
/// Extended to support common subject prefixes used in conditions (including 他のあなたの).
/// Internally handles trait count conditions, level conditions, duration prefixes,
/// and sub-translates the ability text via the registry.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの《サマポケ》のキャラが4枚以上なら、次の相手のターンの終わりまで、このカードは次の能力を得る。『【永】 このカードの正面のキャラのソウルを－1。』</c></para>
/// <para><b>Regex:</b> ^(?:その|この|あなたの|他のあなたの)(?&lt;condition&gt;.+?)なら、(?&lt;ability&gt;.+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>condition: The conditional clause text (e.g., "《サマポケ》のキャラが4枚以上")</description></item>
///   <item><description>ability: The ability text after なら、 (e.g., "次の相手のターンの終わりまで、このカードは次の能力を得る。『...』")</description></item>
/// </list>
/// <para><b>Output:</b> <c>If {conditionEnglish}, {subTranslatedAbility}</c></para>
/// <para><b>Processing:</b></para>
/// <list type="bullet">
///   <item><description>Detects "他のあなたの" prefix → marks <c>hasOther = true</c> for condition text</description></item>
///   <item><description>Checks condition against trait count pattern: <c>《trait》のキャラがN枚以上</c></description></item>
///   <item><description>Checks condition against level pattern: <c>レベルがN(以上|以下)</c></description></item>
///   <item><description>Strips duration prefix (e.g., "次の相手のターンの終わりまで") from ability text</description></item>
///   <item><description>Strips subject prefix (e.g., "このカードは") from ability text</description></item>
///   <item><description>Sub-translates remaining ability text via <see cref="ITokenRegistry.EffectListRegistry"/></description></item>
///   <item><description>Returns <see cref="ConditionalCardEffectAbility"/> wrapping the condition and ability</description></item>
/// </list>
/// <para><b>Scope Expansion:</b> To support additional condition patterns, add new branches in
/// <see cref="TranslateCondition"/> (e.g., stock count, CX existence). For new duration prefixes,
/// add entries to <see cref="DurationMap"/>.</para>
/// </remarks>
internal class ConditionalAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "このカードのバトル中", " during this card's battle" },
        { "あなたのターン中", " during your turn" },
        { "そのアタック中", " during that attack" },
    };

    private static readonly string[] DurationPrefixes = DurationMap.Keys
        .SelectMany(k => new[] { k + "、", k })
        .OrderByDescending(k => k.Length)
        .ToArray();

    public override Regex Matcher => new(@"^(?:その|この|あなたの|他のあなたの)(?<condition>.+?)なら、(?<ability>.+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches =>
    [
        "他のあなたの《サマポケ》のキャラが4枚以上なら、次の相手のターンの終わりまで、このカードは次の能力を得る。『【永】 このカードの正面のキャラのソウルを－1。』"
    ];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var fullText = span.ToString();
        var match = Matcher.Match(fullText);
        var conditionText = match.Groups["condition"].Value;
        var abilityText = match.Groups["ability"].Value.TrimStart(' ', '\t');

        var hasOther = fullText.StartsWith("他のあなたの", StringComparison.Ordinal);

        var conditionEnglish = TranslateCondition(registry, conditionText, hasOther);

        var (strippedAbility, durationText) = StripDuration(abilityText);

        strippedAbility = StripSubjectPrefix(strippedAbility);

        var abilities = SubTranslateAbility(registry, strippedAbility);

        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(durationText))
        {
            var quoteIndex = abilityEnglish.IndexOf(". \"");
            if (quoteIndex >= 0)
                abilityEnglish = abilityEnglish.Insert(quoteIndex, durationText);
            else
                abilityEnglish += durationText;
        }

        var conditions = new List<CardEffectCondition>
        {
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = conditionEnglish
            }
        };

        return
        [
            new ConditionalCardEffectAbility
            {
                ConditionText = conditionEnglish,
                Condition = conditions,
                ActualAbilityText = abilityEnglish,
                AbilityText = abilityEnglish
            }
        ];
    }

    private static string TranslateCondition(ITokenRegistry registry, string conditionText, bool hasOther)
    {
        var otherPhrase = hasOther ? " other" : "";

        var traitMatch = System.Text.RegularExpressions.Regex.Match(conditionText, @"^《(.+?)》のキャラが(\d+)枚以上");
        if (traitMatch.Success)
        {
            var trait = registry.MatchNameFragment(traitMatch.Groups[1].Value);
            var count = traitMatch.Groups[2].Value;
            return $"you have {count} or more{otherPhrase} <<{trait}>> characters";
        }

        var levelMatch = System.Text.RegularExpressions.Regex.Match(conditionText, @"レベルが(\d+)(以上|以下)");
        if (levelMatch.Success)
        {
            var level = levelMatch.Groups[1].Value;
            var comparison = levelMatch.Groups[2].Value == "以上" ? "or higher" : "or lower";
            return $"that card is level {level} {comparison}";
        }

        return conditionText;
    }

    private static (string Remaining, string? DurationText) StripDuration(string abilityText)
    {
        foreach (var prefix in DurationPrefixes)
        {
            if (abilityText.StartsWith(prefix, StringComparison.Ordinal))
            {
                var remaining = abilityText[prefix.Length..].TrimStart('、', ' ', '\t');
                var durKey = prefix.TrimEnd('、');
                if (DurationMap.TryGetValue(durKey, out var durText))
                    return (remaining, durText);
            }
        }
        return (abilityText, null);
    }

    private static string StripSubjectPrefix(string text)
    {
        foreach (var prefix in new[] { "あなたは", "あなたの", "自分の", "このカードは", "このカードが", "相手の", "他の" })
        {
            if (text.StartsWith(prefix, StringComparison.Ordinal))
            {
                return text[prefix.Length..].TrimStart('、', ' ', '\t');
            }
        }
        return text;
    }

    private static List<CardEffectAbility> SubTranslateAbility(ITokenRegistry registry, string abilityText)
    {
        var abilities = new List<CardEffectAbility>();
        var remaining = abilityText;
        var maxIterations = 10;
        var iteration = 0;

        while (!string.IsNullOrWhiteSpace(remaining) && iteration < maxIterations)
        {
            iteration++;
            var trimmed = remaining.TrimStart();
            var matchResult = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (matchResult != null)
            {
                var abilList = matchResult.Translate(registry);
                abilities.AddRange(abilList);
                remaining = trimmed[matchResult.Match.Length..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                var found = false;
                foreach (var prefix in new[] { "このカードを", "そのカードを", "自分の", "相手の", "他の" })
                {
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        remaining = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }
        }

        return abilities;
    }
}
