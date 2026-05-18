namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Matches counter effect (【カウンター】) clauses with optional cost, condition, and effect text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>【カウンター】 助太刀3000 レベル2 ［(1) 手札のこのカードを控え室に置く］</c></para>
/// <para><b>Regex:</b> ^【カウンター】\s*(?&lt;mainText&gt;.+)$</para>
/// <para><b>Parsing Flow:</b></para>
/// <list type="bullet">
///   <item><description>Extracts cost from ［...］ brackets using <c>Match</c> API</description></item>
///   <item><description>Extracts condition from <c>...なら、...</c> pattern using <c>GetMatch</c> API</description></item>
///   <item><description>Parses effect text via <see cref="MultiClauseEffectParser.Parse"/> for multi-sentence ability parsing</description></item>
///   <item><description>Applies opponent-reference post-processing when <c>相手の</c> is present in input</description></item>
///   <item><description>Joins ability parts with <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/></description></item>
/// </list>
/// <para><b>Expected Format:</b> <c>[COUNTER] [cost] condition, effect.</c></para>
/// <para><b>Scope Expansion:</b> Condition parsing uses legacy <c>GetMatch</c> — should migrate to <c>Match</c> API.</para>
/// </remarks>
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
            : registry.ConditionListRegistry.Match(conditionTextJapanese.AsMemory())?.Translate(registry) ?? [];

        // Use MultiClauseEffectParser for ability parsing
        List<ParsedSentence>? parsedList = null;
        if (!string.IsNullOrEmpty(effectTextJapanese))
        {
            parsedList = MultiClauseEffectParser.Parse(effectTextJapanese, registry, MultiClauseEffectParser.DefaultPrefixMap);

            // Post-process opponent references directly on ability texts
            if (mainText.Contains("相手の"))
            {
                var opponentRegex = new Regex(@"(?<!\bother\s+)(?<!\byour\s+opponent's\s+)\byour\b(?!\s+opponent's)");
                parsedList = parsedList.Select(ps => ps with
                {
                    Abilities = ps.Abilities.Select(a =>
                        a with { AbilityText = opponentRegex.Replace(a.AbilityText, "your opponent's", 1) }
                    ).ToList()
                }).ToList();
            }

            foreach (var p in parsedList)
            {
                if (!string.IsNullOrWhiteSpace(p.Remaining) && p.Abilities.Count == 0)
                {
                    Log.Debug("CounterEffect: sentence '{Sentence}' had unparsed remaining: '{Remaining}'", p.Text, p.Remaining);
                }
            }
        }

        var conditionEnglish = conditions.Count > 0 ? conditions.AggregateToString() : "";
        var abilityEnglish = AutoEffectToken.JoinAbilityPartsFromSentences(parsedList ?? []);

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
            Abilities = parsedList?.SelectMany(p => p.Abilities).ToList() ?? [],
            AbilityText = abilityEnglish,
            EffectText = effectText
        };
    }
}
