using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ContEffectToken : CardTextToken<CardEffect>
{
    // Known labels that can appear after 【永】
    private static readonly Dictionary<string, string> LabelMap = new()
    {
        { "応援", "Assist" }
    };

    public override Regex Matcher => new(@"^【永】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var mainText = match.Groups["mainText"].Value.Trim();

        // Check if it has a condition (contains "なら、")
        var conditionMatch = Regex.Match(mainText, @"^(?<condition>.+?)なら、(?<effect>.+)$");

        string conditionTextJapanese = string.Empty;
        string effectTextJapanese = mainText;
        var labels = Array.Empty<string>();

        if (conditionMatch.Success)
        {
            conditionTextJapanese = conditionMatch.Groups["condition"].Value + "なら";
            effectTextJapanese = conditionMatch.Groups["effect"].Value.Trim();
        }
        else
        {
            // Check for labels like "応援" (without 【】)
            var labelMatch = Regex.Match(mainText, @"^(?<label>\S+?)\s+(?<rest>.+)$");
            if (labelMatch.Success)
            {
                var label = labelMatch.Groups["label"].Value;
                if (LabelMap.ContainsKey(label))
                {
                    labels = [LabelMap[label]];
                    effectTextJapanese = labelMatch.Groups["rest"].Value.Trim();
                }
            }
        }

        var conditions = string.IsNullOrEmpty(conditionTextJapanese)
            ? []
            : registry.ConditionListRegistry.GetMatch(conditionTextJapanese)(registry);

        var abilities = registry.EffectListRegistry.GetMatch(effectTextJapanese)(registry);

        var conditionEnglish = conditions.FirstOrDefault()?.ConditionText ?? "";
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        var effectText = "[CONT]";
        if (labels.Length > 0)
            effectText += $" {string.Join("][", labels)}";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        effectText += $" {abilityEnglish}.";

        return new ContCardEffect
        {
            Labels = labels,
            ConditionText = conditionEnglish,
            Condition = conditions,
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
