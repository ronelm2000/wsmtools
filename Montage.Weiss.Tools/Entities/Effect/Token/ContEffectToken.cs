using Montage.Weiss.Tools.Exceptions;

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
///   <item><description>Multiple labels can appear: 記憶 応援, etc.</description></item>
///   <item><description>Conditions are iteratively matched from the start of remaining text</description></item>
///   <item><description>Abilities are iteratively matched from the remaining text after conditions</description></item>
/// </list>
/// </remarks>
internal class ContEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ContEffectToken>();

    // Known labels that can appear after 【永】 — order matters: longer labels first
    private static readonly Dictionary<string, string> LabelMap = new()
    {
        { "応援", "Assist" },
        { "経験", "Experience" },
        { "記憶", "Memory" },
    };

    public override Regex Matcher => new(@"^【永】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var mainText = match.Groups["mainText"].Value.Trim();

        var labels = new List<string>();
        var remainingText = mainText;

        // Check for labels like "応援" (without 【】) — supports multiple labels
        while (true)
        {
            var labelMatch = Regex.Match(remainingText, @"^(?<label>\S+?)\s+(?<rest>.+)$");
            if (labelMatch.Success)
            {
                var label = labelMatch.Groups["label"].Value;
                if (LabelMap.TryGetValue(label, out var englishLabel))
                {
                    labels.Add(englishLabel);
                    remainingText = labelMatch.Groups["rest"].Value.Trim();
                    continue;
                }
            }
            break;
        }

        // Use MultiClauseEffectParser for condition + ability parsing
        var tokenLog = new List<string>();
        var parsedList = MultiClauseEffectParser.Parse(remainingText, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var allConditions = parsedList.SelectMany(p => p.Conditions).ToList();

        // Log warnings for any sentence with unmatched remaining text
        foreach (var p in parsedList)
        {
            if (!string.IsNullOrWhiteSpace(p.Remaining) && p.Abilities.Count == 0)
            {
                Log.Debug("Parse: sentence '{Sentence}' had unparsed remaining: '{Remaining}'", p.Text, p.Remaining);
            }
        }

        var conditions = allConditions;
        foreach (var p in parsedList)
        {
            foreach (var n in p.ConditionTokenNames)
                tokenLog.Add($"Cond:{n}");
            foreach (var n in p.AbilityTokenNames)
                tokenLog.Add($"Abil:{n}");
        }

        Log.Debug("ContEffectToken: parsed {CondCount} conditions, {AbilCount} abilities from '{Rest}'",
            conditions.Count, parsedList.Sum(p => p.Abilities.Count), remainingText);

        var conditionEnglish = conditions.AggregateToString();
        var abilityEnglish = AutoEffectToken.JoinAbilityPartsFromSentences(parsedList);
        
        var effectText = "[CONT]";
        if (labels.Count > 0)
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
        if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith('"') && !abilityForEffect.Contains("get the following abilities"))
            effectText += ".";

        var abilities = parsedList.SelectMany(p => p.Abilities).ToList();
        var unmatchedConditions = conditions.Where(c => c.IsUnmatched).ToList();
        var unmatchedAbilities = abilities.Where(a => a.IsUnmatched).ToList();

        if (unmatchedConditions.Count > 0 || unmatchedAbilities.Count > 0)
        {
            var result = new ContCardEffect
            {
                Labels = labels.ToArray(),
                ConditionText = conditionEnglish,
                Condition = conditions,
                Abilities = abilities,
                AbilityText = abilityEnglish,
                EffectText = effectText,
                TokenLog = tokenLog
            };
            Log.Debug("ContEffectToken: full result = {@Result}", result);
            throw new TranslationNotImplementedException(
                $"Unrecognized [condition(s): {string.Join(" / ", unmatchedConditions.Select(c => c.ConditionText))}]" +
                (unmatchedConditions.Count > 0 && unmatchedAbilities.Count > 0 ? " | " : "") +
                $"[ability(ies): {string.Join(" / ", unmatchedAbilities.Select(a => a.AbilityText))}]",
                result);
        }

        return new ContCardEffect
        {
            Labels = labels.ToArray(),
            ConditionText = conditionEnglish,
            Condition = conditions,
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = effectText,
            TokenLog = tokenLog
        };
    }
}
