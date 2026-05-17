namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ContEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ContEffectToken>();

    private static readonly Dictionary<string, string> LabelMap = new()
    {
        { "応援", "Assist" },
        { "経験", "Experience" }
    };

    public override Regex Matcher => new(@"^【永】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var mainText = match.Groups["mainText"].Value.Trim();

        var labels = Array.Empty<string>();
        var remainingText = mainText;

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

        // Use MultiClauseEffectParser for condition + ability parsing
        var tokenLog = new List<string>();
        var parsed = MultiClauseEffectParser.ParseSentence(remainingText, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var conditions = parsed.Conditions;
        var allAbilities = parsed.Abilities;
        var abilityParts = parsed.Abilities.Select(a => a.AbilityText).ToList();

        Log.Debug("ContEffectToken: parsed {CondCount} conditions, {AbilCount} abilities from '{Rest}'",
            conditions.Count, allAbilities.Count, remainingText);

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
