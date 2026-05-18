namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ActEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【起】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
        var mainText = match.Groups["mainText"].Value.Trim();

        // Extract cost if present: ［...］
        var costMatch = Regex.Match(mainText, @"^［(?<cost>.+?)］\s*(?<rest>.+)$");
        string costTextJapanese = string.Empty;
        string rest = mainText;

        if (costMatch.Success)
        {
            costTextJapanese = costMatch.Groups["cost"].Value;
            rest = costMatch.Groups["rest"].Value.Trim();
        }

        // Translate cost
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? []
            : registry.EffectListRegistry.GetMatch(costTextJapanese.AsMemory())(registry);

        // Iterative ability matching
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();
        var tokenLog = new List<string>();
        var remainingText = rest;

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0)
                break;

            if (registry.EffectListRegistry.TryFindFirstMatch(trimmed, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
            {
                if (matchIndex > 0)
                {
                    remainingText = trimmed[matchIndex..];
                    continue;
                }
                var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
                if (abilMatch != null)
                    tokenLog.Add(abilMatch.Match.Token);
                var abilList = abilFunc(registry);
                allAbilities.AddRange(abilList);
                abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                remainingText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        // Post-process opponent references
        if (mainText.Contains("相手の"))
        {
            var opponentRegex = new Regex(@"(?<!\bother\s+)(?<!\byour\s+opponent's\s+)\byour\b(?!\s+opponent's)");
            abilityParts = abilityParts.Select(part =>
                opponentRegex.Replace(part, "your opponent's", 1)
            ).ToList();
        }

        var abilityEnglish = AutoEffectToken.JoinAbilityParts(abilityParts);

        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];

        var prefixParts = new List<string> { "[ACT]" };
        if (labels.Length > 0)
            prefixParts.AddRange(labels.Select(label => $"[{label}]"));
        if (!string.IsNullOrEmpty(costEnglish))
            prefixParts.Add($"[{costEnglish}]");
        
        var effectText = string.Join("", prefixParts);
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            if (labels.Length > 0 && abilityForEffect.Length > 0)
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            effectText += $" {abilityForEffect}";
        }
        if (!effectText.TrimEnd().EndsWith("."))
            effectText += ".";

        var finalLabels = labels;
        if (Regex.IsMatch(mainText, @"^集中"))
            finalLabels = [.. finalLabels, "Brainstorm"];

        return new ActCardEffect {
                Labels = finalLabels,
                CostText = costEnglish,
                Cost = costAbilities,
                Abilities = allAbilities,
                AbilityText = abilityEnglish,
                EffectText = effectText,
                TokenLog = tokenLog
        };
    }
}
