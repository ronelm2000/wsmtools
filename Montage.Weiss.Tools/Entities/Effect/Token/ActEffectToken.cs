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
        string conditionEnglish = "";
        List<ParsedSentence> parsedList;

        // Handle backup/counter pattern: 助太刀{N} レベル{M}［{cost}］
        // where cost is embedded inside the ［...］ rather than being at the start of mainText
        var backupMatch = Regex.Match(rest, @"^助太刀([Ｘ\d]+)\s*レベル(\d+)\s*［(?:\((\d+)\)\s*)?(.+?)］(?:\s*(?<postcondition>.+))?$");
        var isBackup = backupMatch.Success;
        string backupPower = "", backupLevel = "";
        if (isBackup)
        {
            backupPower = backupMatch.Groups[1].Value.Replace("Ｘ", "X");
            backupLevel = backupMatch.Groups[2].Value;
            var stockCost = backupMatch.Groups[3].Success ? $"({backupMatch.Groups[3].Value}) " : "";
            var postCondition = backupMatch.Groups["postcondition"].Success ? backupMatch.Groups["postcondition"].Value.Trim() : "";
            abilityEnglish = $"[{stockCost}Put this card in your hand to your waiting room]";
            if (!string.IsNullOrEmpty(postCondition))
            {
                var postMatch = registry.ConditionListRegistry.Match(postCondition.AsMemory());
                if (postMatch != null)
                {
                    var postConds = postMatch.Translate(registry).Where(c => c.Type == ConditionType.PostCondition).ToList();
                    if (postConds.Count > 0)
                    {
                        abilityEnglish += " " + string.Join(". ", postConds.Select(c => c.ConditionText));
                    }
                }
            }
            costEnglish = "";
            parsedList = [];
            tokenLog.Add("Abil:BackupPrefix");
        }
        else
        {
            parsedList = MultiClauseEffectParser.Parse(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);

            foreach (var p in parsedList)
            {
                foreach (var n in p.ConditionTokenNames)
                    tokenLog.Add($"Cond:{n}");
                foreach (var n in p.AbilityTokenNames)
                    tokenLog.Add($"Abil:{n}");
            }

            abilityEnglish = AutoEffectToken.JoinAbilityPartsFromSentences(parsedList);

            // Extract first-sentence conditions for effect text
            var firstSentenceConditions = parsedList.Count > 0
                ? parsedList[0].Conditions.Where(c => c.Type != ConditionType.PostCondition).ToList()
                : [];
            conditionEnglish = firstSentenceConditions.Count > 0
                ? firstSentenceConditions.AggregateToString()
                : "";

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
        // Build effect text: [ACT][bracket-labels] raw-label [cost] ability.
        // Bracket-enclosed labels (e.g. [COUNTER]) are joined as [ACT][COUNTER];
        // raw labels (e.g. "Backup 1000, Level 1") appear after with a space;
        // Brainstorm appears as space-separated, not in ][ format.
        var bracketLabels = new List<string> { "ACT" };
        var rawLabels = new List<string>();
        foreach (var lbl in labels)
        {
            if (lbl.StartsWith('[') && lbl.EndsWith(']'))
                bracketLabels.Add(lbl.Trim('[', ']'));
            else
                rawLabels.Add(lbl);
        }
        if (hasBrainstorm)
        {
            rawLabels.Add("Brainstorm");
        }
        if (isBackup)
        {
            rawLabels.Add($"Backup {backupPower}, Level {backupLevel}");
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
            if (!string.IsNullOrEmpty(conditionEnglish))
            {
                var condUpper = char.ToUpper(conditionEnglish[0]) + conditionEnglish[1..];
                var secondPart = abilityForEffect.Length > 0 && char.IsUpper(abilityForEffect[0])
                    ? char.ToLower(abilityForEffect[0]) + abilityForEffect[1..]
                    : abilityForEffect;
                abilityForEffect = $"{condUpper}, {secondPart}";
            }
            else
            {
                if (abilityForEffect.Length > 0)
                    abilityForEffect = char.ToUpper(abilityForEffect[0]) + abilityForEffect[1..];
            }
            effectText += $" {abilityForEffect}";
        }
        if (!effectText.TrimEnd().EndsWith(".") && !effectText.EndsWith("]") && !effectText.EndsWith("\""))
            effectText += ".";

        return new ActCardEffect {
                Labels = [.. labels, .. rawLabels],
                CostText = costEnglish,
                Cost = costAbilities,
                Abilities = parsedList.SelectMany(p => p.Abilities).ToList(),
                AbilityText = abilityEnglish,
                EffectText = effectText,
                TokenLog = tokenLog
        };
    }
}
