namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class CounterEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CounterEffectToken>();

    public override Regex Matcher => new(@"^【カウンター】\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
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

        // Translate cost using Match API
        var costAbilities = new List<CardEffectAbility>();
        if (!string.IsNullOrEmpty(costTextJapanese))
        {
            var costRemaining = costTextJapanese;
            while (!string.IsNullOrWhiteSpace(costRemaining))
            {
                var t = costRemaining.TrimStart();
                var m = registry.EffectListRegistry.Match(t.AsMemory());
                if (m == null) break;
                costAbilities.AddRange(m.Translate(registry));
                costRemaining = t[m.Match.Length..].TrimStart('、', ' ', '\t');
            }
        }

        // Check for condition
        var conditionMatch = Regex.Match(rest, @"^(?<condition>.+?)なら、?(?<effect>.+)$");
        string conditionTextJapanese = string.Empty;
        string effectTextJapanese = rest;

        if (conditionMatch.Success)
        {
            conditionTextJapanese = conditionMatch.Groups["condition"].Value + "なら";
            effectTextJapanese = conditionMatch.Groups["effect"].Value.Trim();
        }

        var conditions = string.IsNullOrEmpty(conditionTextJapanese)
            ? []
            : registry.ConditionListRegistry.GetMatch(conditionTextJapanese.AsMemory())(registry);

        // Use MultiClauseEffectParser for ability parsing
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();
        if (!string.IsNullOrEmpty(effectTextJapanese))
        {
            var parsedList = MultiClauseEffectParser.Parse(effectTextJapanese, registry, MultiClauseEffectParser.DefaultPrefixMap);
            allAbilities = parsedList.SelectMany(p => p.Abilities).ToList();
            abilityParts = allAbilities.Select(a => a.AbilityText).ToList();

            foreach (var p in parsedList)
            {
                if (!string.IsNullOrWhiteSpace(p.Remaining) && p.Abilities.Count == 0)
                {
                    Log.Debug("CounterEffect: sentence '{Sentence}' had unparsed remaining: '{Remaining}'", p.Text, p.Remaining);
                }
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

        var conditionEnglish = conditions.Count > 0 ? conditions.AggregateToString() : "";
        var abilityEnglish = AutoEffectToken.JoinAbilityParts(abilityParts);

        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];

        var effectText = "[COUNTER]";
        if (!string.IsNullOrEmpty(costEnglish))
            effectText += $" [{costEnglish}]";
        if (!string.IsNullOrEmpty(conditionEnglish))
            effectText += $" {conditionEnglish},";
        else if (!string.IsNullOrEmpty(costEnglish))
            effectText += " ";
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            var abilityForEffect = abilityEnglish;
            if (!string.IsNullOrEmpty(conditionEnglish) && abilityForEffect.Length > 0)
                abilityForEffect = char.ToLower(abilityForEffect[0]) + abilityForEffect[1..];
            effectText += $" {abilityForEffect}";
            if (!abilityForEffect.EndsWith('.') && !abilityForEffect.EndsWith('"'))
                effectText += ".";
        }

        return new EventCardEffect
        {
            Labels = ["COUNTER"],
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
