namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches conditional ability patterns where a condition precedes an ability action.
/// Pattern: [condition]なら、[ability]
/// Example: そのカードのレベルが1以上なら、あなたはこのカードをストック置場に置いてよい。
/// </summary>
internal class ConditionalAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    // Match conditional ability patterns where a condition precedes an ability action.
    // Only matches patterns starting with specific pronouns to avoid over-matching.
    // Pattern: その/この/あなたの[condition]なら、[ability]
    public override Regex Matcher => new(@"^(?:その|この|あなたの)(?<condition>.+?)なら、(?<ability>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var conditionText = match.Groups["condition"].Value;
        var abilityText = match.Groups["ability"].Value.TrimStart(' ', '\t');

        // Strip known subject prefixes from ability text
        var strippedAbility = abilityText;
        foreach (var prefix in new[] { "あなたは", "あなたの", "自分の", "このカードは", "このカードが", "相手の", "他の" })
        {
            if (strippedAbility.StartsWith(prefix, StringComparison.Ordinal))
            {
                strippedAbility = strippedAbility[prefix.Length..].TrimStart('、', ' ', '\t');
                break;
            }
        }

        // Try to translate ability using registry
        var abilities = new List<CardEffectAbility>();
        var abilRemaining = strippedAbility;
        var maxIterations = 10;
        var iteration = 0;

        while (!string.IsNullOrWhiteSpace(abilRemaining) && iteration < maxIterations)
        {
            iteration++;
            var trimmed = abilRemaining.TrimStart();
            var matchResult = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (matchResult != null)
            {
                var abilList = matchResult.Translate(registry);
                abilities.AddRange(abilList);
                abilRemaining = trimmed[matchResult.Match.Length..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                // Try to find a match anywhere in the text
                var found = false;
                foreach (var prefix in new[] { "このカードを", "そのカードを", "自分の", "相手の", "他の" })
                {
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        abilRemaining = trimmed[prefix.Length..].TrimStart('、', ' ', '\t');
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }
        }

        // Build condition English text (without "If" prefix - AggregateToString adds it)
        string conditionEnglish;
        var levelMatch = System.Text.RegularExpressions.Regex.Match(conditionText, @"レベルが(\d+)(以上|以下)");
        if (levelMatch.Success)
        {
            var level = levelMatch.Groups[1].Value;
            var comparison = levelMatch.Groups[2].Value == "以上" ? "or higher" : "or lower";
            conditionEnglish = $"that card is level {level} {comparison}";
        }
        else
        {
            conditionEnglish = conditionText;
        }

        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        var conditions = new List<CardEffectCondition>
        {
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = conditionEnglish
            }
        };

        return
        [
            new ConditionalCardEffectAbility
            {
                ConditionText = conditionEnglish,
                Condition = conditions,
                ActualAbilityText = abilityEnglish,
                AbilityText = abilityEnglish
            }
        ];
    }
}
