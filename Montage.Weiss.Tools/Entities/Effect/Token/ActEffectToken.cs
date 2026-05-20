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

        string costEnglish;
        string abilityEnglish;
        List<ParsedSentence> parsedList;

        // Handle backup/counter pattern: 助太刀{N} レベル{M}［{cost}］
        // where cost is embedded inside the ［...］ rather than being at the start of mainText
        var backupMatch = Regex.Match(rest, @"^助太刀(\d+)\s*レベル(\d+)\s*［(?:\((\d+)\)\s*)?(.+)］$");
        var isBackup = backupMatch.Success;
        string backupPower = "", backupLevel = "";
        if (isBackup)
        {
            backupPower = backupMatch.Groups[1].Value;
            backupLevel = backupMatch.Groups[2].Value;
            var stockCost = backupMatch.Groups[3].Success ? $"({backupMatch.Groups[3].Value}) " : "";
            abilityEnglish = $"[{stockCost}Put this card in your hand to your waiting room]";
            costEnglish = "";
            parsedList = [];
            tokenLog.Add("Abil:BackupPrefix");
        }
        else
        {
            parsedList = MultiClauseEffectParser.Parse(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);

            foreach (var a in parsedList.SelectMany(p => p.Abilities))
                tokenLog.Add($"Abil:{a.GetType().Name}");

            abilityEnglish = AutoEffectToken.JoinAbilityPartsFromSentences(parsedList);

            var costTexts = costAbilities.Select(a => a.AbilityText).ToList();
            costEnglish = "";
            if (costTexts.Count > 0)
            {
                costEnglish = costTexts[0];
                for (int i = 1; i < costTexts.Count; i++)
                {
                    var sep = i == 1 && Regex.IsMatch(costTexts[0], @"^\(\d+\)$") ? " " : " & ";
                    var nextText = AutoEffectToken.CapitalizeFirstAlpha(costTexts[i]);
                    costEnglish += sep + nextText;
                }
            }
            costEnglish = AutoEffectToken.CapitalizeFirstAlpha(costEnglish);
        }

        // Build label list (MatchLabels already returns correct format, e.g. "[COUNTER]" or "Brainstorm")
        var finalLabelList = new List<string>(labels);
        if (hasBrainstorm)
            finalLabelList.Add("Brainstorm");
        if (isBackup)
            finalLabelList.Add($"Backup {backupPower}, Level {backupLevel}");
        var finalLabels = finalLabelList.ToArray();

        // Build effect text: [ACT][bracket-labels] raw-label [cost] ability.
        // Bracket-enclosed labels (e.g. [COUNTER]) are joined as [ACT][COUNTER];
        // raw labels (e.g. "Backup 1000, Level 1") appear after with a space;
        // Brainstorm appears as space-separated, not in ][ format.
        var bracketLabels = new List<string> { "ACT" };
        var rawLabels = new List<string>();
        foreach (var lbl in finalLabels)
        {
            if (lbl.StartsWith('[') && lbl.EndsWith(']'))
                bracketLabels.Add(lbl.Trim('[', ']'));
            else
                rawLabels.Add(lbl);
        }
        var effectText = $"[{string.Join("][", bracketLabels)}]";
        if (rawLabels.Count > 0)
            effectText += $" {string.Join(" ", rawLabels)}";
        if (!string.IsNullOrEmpty(costEnglish))
        {
            var costSpace = rawLabels.Count > 0 || bracketLabels.Count > 1 ? " " : "";
            effectText += $"{costSpace}[{costEnglish}]";
        }
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            if (abilityForEffect.Length > 0)
                abilityForEffect = char.ToUpper(abilityForEffect[0]) + abilityForEffect[1..];
            effectText += $" {abilityForEffect}";
        }
        if (!effectText.TrimEnd().EndsWith(".") && !effectText.EndsWith("]") && !effectText.EndsWith("\""))
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
