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

        // Attempt 1: MultiClauseEffectParser.Parse (handles multi-sentence text)
        var parsedSentences = MultiClauseEffectParser.Parse(input, registry, MultiClauseEffectParser.DefaultPrefixMap);
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
            var joined = string.Join(". ", parts.Select(s => s.TrimEnd('.')));
            joined = char.ToUpper(joined[0]) + joined[1..];
            var trimmed = joined.TrimEnd('"');
            if (!trimmed.EndsWith('.'))
                joined += ".";

                return new EventCardEffect
                {
                    Labels = labels,
                    EffectText = joined,
                    AbilityText = joined,
                    Abilities = allAbilities
                };
            }
        }

        // Attempt 2: ParseSentence (single sentence with conditions + abilities)
        var parsed = MultiClauseEffectParser.ParseSentence(input, registry, MultiClauseEffectParser.DefaultPrefixMap);
        if (parsed.Abilities.Count > 0 || parsed.Conditions.Count > 0)
        {
            Log.Debug("EventEffectToken: ParseSentence produced {abilCount} abilities", parsed.Abilities.Count);

            return new EventCardEffect
            {
                Labels = labels,
                EffectText = parsed.Text,
                AbilityText = parsed.Text,
                Abilities = parsed.Abilities
            };
        }

        // Attempt 3: Direct ability match (pure ability text, no conditions)
        var abilMatch = registry.EffectListRegistry.Match(input.AsMemory());
        if (abilMatch != null)
        {
            var abils = abilMatch.Translate(registry);
            var abilityEnglish = string.Join(", ", abils.Select(a => a.AbilityText));

            Log.Debug("EventEffectToken: ability match produced {count} abilities", abils.Count);

            return new EventCardEffect
            {
                Labels = labels,
                EffectText = abilityEnglish,
                AbilityText = abilityEnglish,
                Abilities = abils
            };
        }

        // Fallback: unmatched ability
        Log.Debug("EventEffectToken: no match found, creating unmatched fallback");
        return new EventCardEffect
        {
            Labels = labels,
            EffectText = input,
            AbilityText = input,
            Abilities = [new UnmatchedAbility
            {
                AbilityText = input,
                IsUnmatched = true,
                Suggestions = ["unmatched nested ability text"]
            }]
        };
    }
}
