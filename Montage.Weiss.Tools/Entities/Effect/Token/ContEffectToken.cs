namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ContEffectToken : CardTextToken<CardEffect>
{
    // Known labels that can appear after 【永】
    private static readonly Dictionary<string, string> LabelMap = new()
    {
        { "応援", "Assist" },
        { "経験", "Experience" }
    };

    public override Regex Matcher => new(@"^【永】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
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

        while (true)
        {
            var trimmed = remainingText.TrimStart();
            if (registry.ConditionListRegistry.TryMatchAtStart(trimmed, out var condFunc, out var consumed) && condFunc != null)
            {
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
                var abilList = abilFunc(registry);
                allAbilities.AddRange(abilList);
                abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                abilityText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        var conditionTexts = conditions.Select(c => c.ConditionText).Where(c => !string.IsNullOrEmpty(c)).ToList();
        for (int i = 0; i < conditionTexts.Count; i++)
        {
            if (conditionTexts[i].Length == 0) continue;
            var startsWithConditional = conditionTexts[i].StartsWith("If") || conditionTexts[i].StartsWith("When") || conditionTexts[i].StartsWith("During") || conditionTexts[i].StartsWith("At");
            if (!startsWithConditional)
            {
                if (i == 0)
                    conditionTexts[i] = "If " + conditionTexts[i];
                else
                    conditionTexts[i] = "if " + conditionTexts[i];
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
            EffectText = effectText
        };
    }
}
