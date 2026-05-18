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
        var hasBrainstorm = mainText.StartsWith("集中", StringComparison.Ordinal);

        // Handle Brainstorm label prefix before cost extraction
        if (hasBrainstorm)
        {
            mainText = Regex.Replace(mainText, @"^集中\s*", "");
        }

        // Extract cost if present: ［...］
        var costMatch = Regex.Match(mainText, @"^［(?<cost>.+?)］\s*(?<rest>.+)$");
        string costTextJapanese = string.Empty;
        string rest = mainText;

        if (costMatch.Success)
        {
            costTextJapanese = costMatch.Groups["cost"].Value;
            rest = costMatch.Groups["rest"].Value.Trim();
        }

        // Translate cost using Match API
        var costAbilities = new List<CardEffectAbility>();
        var tokenLog = new List<string>();
        if (!string.IsNullOrEmpty(costTextJapanese))
        {
            var costRemaining = costTextJapanese;
            while (!string.IsNullOrWhiteSpace(costRemaining))
            {
                var t = costRemaining.TrimStart();
                var m = registry.EffectListRegistry.Match(t.AsMemory());
                if (m == null) break;
                var abils = m.Translate(registry);
                costAbilities.AddRange(abils);
                tokenLog.Add($"Cost:{m.Match.Token}");
                costRemaining = t[m.Match.Length..].TrimStart('、', ' ', '\t');
            }
        }

        // Use MultiClauseEffectParser for ability parsing (supports multi-sentence effects like Brainstorm)
        var parsedList = MultiClauseEffectParser.Parse(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);

        foreach (var a in parsedList.SelectMany(p => p.Abilities))
            tokenLog.Add($"Abil:{a.GetType().Name}");

        var abilityEnglish = AutoEffectToken.JoinAbilityPartsFromSentences(parsedList);

        var costTexts = costAbilities.Select(a => a.AbilityText).ToList();
        var costEnglish = "";
        if (costTexts.Count > 0)
        {
            costEnglish = costTexts[0];
            for (int i = 1; i < costTexts.Count; i++)
            {
                var sep = i == 1 && Regex.IsMatch(costTexts[0], @"^\(\d+\)$") ? " " : " & ";
                costEnglish += sep + costTexts[i];
            }
        }

        var finalLabels = labels;
        if (hasBrainstorm)
            finalLabels = [.. finalLabels, "Brainstorm"];

        var prefixParts = new List<string> { "[ACT]" };
        foreach (var label in finalLabels)
            prefixParts.Add($" {label}");
        if (!string.IsNullOrEmpty(costEnglish))
        {
            var space = finalLabels.Length > 0 ? " " : "";
            prefixParts.Add($"{space}[{costEnglish}]");
        }
        
        var effectText = string.Join("", prefixParts);
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            effectText += $" {abilityEnglish}";
        }
        if (!effectText.TrimEnd().EndsWith("."))
            effectText += ".";

        return new ActCardEffect {
                Labels = finalLabels,
                CostText = costEnglish,
                Cost = costAbilities,
                Abilities = parsedList.SelectMany(p => p.Abilities).ToList(),
                AbilityText = abilityEnglish,
                EffectText = effectText,
                TokenLog = tokenLog
        };
    }
}
