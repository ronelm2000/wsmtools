namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ActEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ActEffectToken>();
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

        // Translate cost using Match API (same pattern as AutoEffectToken)
        var costAbilities = string.IsNullOrEmpty(costTextJapanese)
            ? new List<CardEffectAbility>()
            : ParseCostText(registry, costTextJapanese);

        // Parse the rest using MultiClauseEffectParser
        var tokenLog = new List<string>();
        var parsed = MultiClauseEffectParser.ParseSentence(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var allAbilities = parsed.Abilities;
        var abilityParts = parsed.Abilities.Select(a => a.AbilityText).ToList();

        Log.Debug("ActEffectToken: parsed {AbilCount} abilities from '{Rest}'", allAbilities.Count, rest);

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

        var parts = new List<string> { "[ACT]" };
        if (labels.Length > 0)
            parts.AddRange(labels.Select(label => $"[{label}]"));
        if (!string.IsNullOrEmpty(costEnglish))
            parts.Add($"[{costEnglish}]");
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            if (labels.Length > 0 && abilityForEffect.Length > 0)
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            parts.Add(abilityForEffect);
        }
        var effectText = labels.Length > 0 ? string.Join(" ", parts) : string.Join("", parts);
        if (!string.IsNullOrEmpty(abilityEnglish))
            effectText += " " + abilityEnglish;
        if (!string.IsNullOrEmpty(abilityEnglish) && !effectText.TrimEnd().EndsWith("."))
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

    private static List<CardEffectAbility> ParseCostText(ITokenRegistry registry, string costText)
    {
        var costAbilities = new List<CardEffectAbility>();
        var costRemaining = costText;
        while (!string.IsNullOrWhiteSpace(costRemaining))
        {
            var t = costRemaining.TrimStart();
            var matchResult = registry.EffectListRegistry.Match(t.AsMemory());
            if (matchResult == null) break;

            var abils = matchResult.Translate(registry);
            costAbilities.AddRange(abils);
            costRemaining = t[matchResult.Match.Length..].TrimStart('、', ' ', '\t');
        }
        return costAbilities;
    }
}
