namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Matches event-type effects (no <c>【自】</c>/<c>【永】</c>/<c>【起】</c> prefix).
/// Uses <see cref="MultiClauseEffectParser"/> to parse conditions and abilities,
/// with fallback to <see cref="IComponentRegistry{T}.Match"/> for pure ability text.
/// Also serves as the catch-all fallback for any unmatched text in <see cref="IComponentRegistry{T}"/>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分のキャラを1枚選び、そのターン中、次の能力を与える。『...』</c></para>
/// <para><b>Regex:</b> ^(?&lt;labels&gt;(?:【[^】]+】)*)\s*(?&lt;mainText&gt;.+)$</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>labels: Zero or more 【label】 prefixes (e.g., 【経験】)</description></item>
///   <item><description>mainText: The effect body after labels</description></item>
/// </list>
/// <para><b>Output:</b> Full English effect text with labels, conditions, and abilities joined.</para>
/// </remarks>
internal class EventEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<EventEffectToken>();

    public override Regex Matcher => new(@"^(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$");

    public override IEnumerable<string> SampleMatches => ["あなたは自分の《★TESTTRAIT★》のキャラを1枚選び、そのターン中、パワーを＋2000。"];

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var labels = registry.MatchLabels(match.Groups["labels"]?.Value ?? "");
        var input = match.Groups["mainText"].Value.Trim();

        Log.Debug("EventEffectToken: input='{Input}' labels='{Labels}'", input, (object)labels);

        // Extract cost if present: ［...］
        var costMatch = Regex.Match(input, @"^［(?<cost>.+?)］\s*(?<rest>.+)$");
        string costTextJapanese = string.Empty;
        string rest = input;

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
                var abils = m.Translate(registry);
                costAbilities.AddRange(abils);
                costRemaining = t[m.Match.Length..].TrimStart('、', ' ', '\t');
            }
        }

        var costTexts = costAbilities.Select(a => a.AbilityText).ToList();
        var costEnglish = "";
        if (costTexts.Count > 0)
        {
            costEnglish = costTexts[0];
            for (int i = 1; i < costTexts.Count; i++)
            {
                var sep = i == 1 && Regex.IsMatch(costTexts[0], @"^\(\d+\)$") ? " " : " & ";
                var nextText = AutoEffectToken.CapitalizeFirstAlpha(costTexts[i]);
                costEnglish += sep + nextText;
            }
            costEnglish = AutoEffectToken.CapitalizeFirstAlpha(costEnglish);
        }

        // Attempt 1: MultiClauseEffectParser.Parse (handles multi-sentence text)
        var parsedSentences = MultiClauseEffectParser.Parse(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        if (parsedSentences.Count > 1 || (parsedSentences.Count == 1 && (parsedSentences[0].Abilities.Count > 0 || parsedSentences[0].Conditions.Count > 0)))
        {
            var allAbilities = parsedSentences.SelectMany(s => s.Abilities).ToList();
            var parts = parsedSentences
                .Where(s => s.Abilities.Count > 0 || s.Conditions.Count > 0)
                .Select(s => s.Text)
                .ToList();

            Log.Debug("EventEffectToken: Parse produced {count} parts, {abilCount} abilities", parts.Count, allAbilities.Count);

            if (parts.Count > 0)
            {
                var parts2 = parts.Select(s => s.TrimEnd('.')).ToArray();
                var joined = parts2[0];
                for (int i = 1; i < parts2.Length; i++)
                {
                    var next = parts2[i];
                    if (next.Length > 0 && char.IsUpper(next[0]))
                        next = char.ToLower(next[0]) + next[1..];
                    joined += ". " + next;
                }
                var trimmed = joined.TrimEnd('"');
                if (!trimmed.EndsWith('.'))
                    joined += ".";

                var effectText = string.IsNullOrEmpty(costEnglish) ? joined : $"[{costEnglish}] {joined}";

                return new EventCardEffect
                {
                    Labels = labels,
                    CostText = costEnglish,
                    Cost = costAbilities,
                    EffectText = effectText,
                    AbilityText = joined,
                    Abilities = allAbilities
                };
            }
        }

        // Attempt 2: ParseSentence (single sentence with conditions + abilities)
        var parsed = MultiClauseEffectParser.ParseSentence(rest, registry, MultiClauseEffectParser.DefaultPrefixMap);
        if (parsed.Abilities.Count > 0 || parsed.Conditions.Count > 0)
        {
            Log.Debug("EventEffectToken: ParseSentence produced {abilCount} abilities", parsed.Abilities.Count);

            var effectText = string.IsNullOrEmpty(costEnglish) ? parsed.Text : $"[{costEnglish}] {parsed.Text}";

            return new EventCardEffect
            {
                Labels = labels,
                CostText = costEnglish,
                Cost = costAbilities,
                EffectText = effectText,
                AbilityText = parsed.Text,
                Abilities = parsed.Abilities
            };
        }

        // Attempt 3: Direct ability match (pure ability text, no conditions)
        var abilMatch = registry.EffectListRegistry.Match(rest.AsMemory());
        if (abilMatch != null)
        {
            var abils = abilMatch.Translate(registry);
            var abilityEnglish = string.Join(", ", abils.Select(a => a.AbilityText));

            Log.Debug("EventEffectToken: ability match produced {count} abilities", abils.Count);

            var effectText = string.IsNullOrEmpty(costEnglish) ? abilityEnglish : $"[{costEnglish}] {abilityEnglish}";

            return new EventCardEffect
            {
                Labels = labels,
                CostText = costEnglish,
                Cost = costAbilities,
                EffectText = effectText,
                AbilityText = abilityEnglish,
                Abilities = abils
            };
        }

        // Fallback: unmatched ability
        Log.Debug("EventEffectToken: no match found, creating unmatched fallback");
        var fallbackEffectText = string.IsNullOrEmpty(costEnglish) ? rest : $"[{costEnglish}] {rest}";
        return new EventCardEffect
        {
            Labels = labels,
            CostText = costEnglish,
            Cost = costAbilities,
            EffectText = fallbackEffectText,
            AbilityText = rest,
            Abilities = [new UnmatchedAbility
            {
                AbilityText = rest,
                IsUnmatched = true,
                Suggestions = ["unmatched nested ability text"]
            }]
        };
    }
}
