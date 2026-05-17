namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Matches continuous effect (【永】) clauses and parses their conditions and abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。</c></para>
/// <para><b>Regex:</c> ^【永】\s*(?&lt;mainText&gt;.+)$</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>mainText: All text after 【永】</description></item>
/// </list>
/// <para><b>Expected Full English Format:</b></para>
/// <code>[CONT] [Labels] During [Conditions], when [Conditions], if [Condition], [Ability].</code>
/// <para><b>Notes:</b></para>
/// <list type="bullet">
///   <item><description>Labels like 応援 (Assist), 経験 (Experience) are parsed without brackets</description></item>
///   <item><description>Conditions are iteratively matched from the start of remaining text</description></item>
///   <item><description>Abilities are iteratively matched from the remaining text after conditions</description></item>
/// </list>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different effect type indicators (【継続】, 【常駐])
/// - Different label formats (labels with brackets, multiple labels)</para>
/// </remarks>
internal class ContEffectToken : CardTextToken<CardEffect>
{
    // Known labels that can appear after 【永】
    private static readonly Dictionary<string, string> LabelMap = new()
    {
        { "応援", "Assist" },
        { "経験", "Experience" }
    };

    private static readonly string[] ConditionalLeadInPrefixes =
    [
        "そして、",
        "そして",
        "し、",
        "し"
    ];

    public override Regex Matcher => new(@"^【永】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var mainText = match.Groups["mainText"].Value.Trim();

        var labels = Array.Empty<string>();
        var remainingText = mainText;

        // Expected Format:
        // [CONT] [Labels] During [Conditions], when [Conditions], if [Condition], [Ability].
        // During [Conditions] are only for Type="During" effects, but we can still parse them as conditions for consistency.
        // When [Conditions] are only for Type="When" effects, but we can still parse them as conditions for consistency.
        // If [Condition] is present, it's typically for Type="If" effects, but we can still parse it as a condition for consistency.

        // Check for labels like "応援" (without 【】)
        var labelMatch = Regex.Match(remainingText, @"^(?<label>\S+?)\s+(?<rest>.+)$");
        if (labelMatch.Success)
        {
            var label = labelMatch.Groups["label"].Value;
            if (LabelMap.ContainsKey(label))
            {
                labels = [LabelMap[label]];
                remainingText = labelMatch.Groups["rest"].Value.Trim();
            }
        }

        // Iteratively consume conditions from start of remaining text
        var conditions = new List<CardEffectCondition>();
        var tokenLog = new List<string>();

        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (registry.ConditionListRegistry.TryMatchAtStart(trimmed, out var condFunc, out var consumed) && condFunc != null)
            {
                var condMatch = registry.ConditionListRegistry.Match(trimmed.AsMemory());
                if (condMatch != null)
                    tokenLog.Add(condMatch.Match.Token);
                var condList = condFunc(registry);
                conditions.AddRange(condList);
                remainingText = trimmed[consumed..].TrimStart('、', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        // Translate remaining text as abilities (iterative matching)
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();
        var abilityText = remainingText;

        while (!string.IsNullOrWhiteSpace(abilityText))
        {
            var trimmed = abilityText.TrimStart();
            if (trimmed.Length == 0)
                break;

            if (registry.EffectListRegistry.TryFindFirstMatch(trimmed, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
            {
                if (matchIndex > 0)
                {
                    abilityText = trimmed[matchIndex..];
                    continue;
                }
                var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
                if (abilMatch != null)
                    tokenLog.Add(abilMatch.Match.Token);
                var abilList = abilFunc(registry);
                allAbilities.AddRange(abilList);
                abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                abilityText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                var stripped = false;
                foreach (var prefix in ConditionalLeadInPrefixes)
                {
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        abilityText = trimmed[prefix.Length..];
                        stripped = true;
                        break;
                    }
                }
                if (stripped) continue;

                throw new NotImplementedException($"Unrecognized ability text: {trimmed}");
            }
        }

        var conditionTexts = conditions.Select(c => c.ConditionText).Where(c => !string.IsNullOrEmpty(c)).ToList();
        for (int i = 0; i < conditionTexts.Count; i++)
        {
            if (conditionTexts[i].Length == 0) continue;
            var startsWithConditional = conditionTexts[i].StartsWith("If", StringComparison.OrdinalIgnoreCase) || conditionTexts[i].StartsWith("When", StringComparison.OrdinalIgnoreCase) || conditionTexts[i].StartsWith("During", StringComparison.OrdinalIgnoreCase) || conditionTexts[i].StartsWith("At", StringComparison.OrdinalIgnoreCase) || conditionTexts[i].StartsWith("For", StringComparison.OrdinalIgnoreCase);
            if (!startsWithConditional)
            {
                if (i == 0)
                {
                    conditionTexts[i] = "If " + char.ToLower(conditionTexts[i][0]) + conditionTexts[i][1..];
                }
                else
                {
                    conditionTexts[i] = "if " + char.ToLower(conditionTexts[i][0]) + conditionTexts[i][1..];
                }
            }
            else if (i == 0)
            {
                conditionTexts[i] = char.ToUpper(conditionTexts[i][0]) + conditionTexts[i][1..];
            }
        }
        for (int i = 1; i < conditionTexts.Count; i++)
        {
            if (conditionTexts[i].Length > 0)
                conditionTexts[i] = char.ToLower(conditionTexts[i][0]) + conditionTexts[i][1..];
        }
        var conditionEnglish = string.Join(", ", conditionTexts);
        var abilityEnglish = AutoEffectToken.JoinAbilityParts(abilityParts);
        
        var effectText = "[CONT]";
        if (labels.Length > 0)
            effectText += $" {string.Join("][", labels)}";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        
        var abilityForEffect = abilityEnglish;
        if (abilityForEffect.Length > 0)
        {
            if (string.IsNullOrEmpty(conditionEnglish))
                abilityForEffect = char.ToUpper(abilityForEffect[0]) + abilityForEffect[1..];
            else
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
        }
        effectText += $" {abilityForEffect}";
        if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith('"'))
            effectText += ".";

        return new ContCardEffect
        {
            Labels = labels,
            ConditionText = conditionEnglish,
            Condition = conditions,
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText,
            TokenLog = tokenLog
        };
    }
}
