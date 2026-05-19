namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class BrainstormEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^集中\s*(?<rest>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var rest = match.Groups["rest"].Value.Trim();
        var sentences = rest.Split('。', StringSplitOptions.RemoveEmptyEntries);
        var translatedSentences = new List<string>();
        var allAbilities = new List<CardEffectAbility>();

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var abilityParts = new List<string>();
            var remainingText = trimmed;

            while (!string.IsNullOrWhiteSpace(remainingText))
            {
                var t = remainingText.TrimStart();
                if (t.Length == 0)
                    break;

                // Try matching abilities first
                if (registry.EffectListRegistry.TryFindFirstMatch(t, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
                {
                    if (matchIndex > 0)
                    {
                        remainingText = t[matchIndex..];
                        continue;
                    }
                    var abilList = abilFunc(registry);
                    allAbilities.AddRange(abilList);
                    abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                    remainingText = t[consumed..].TrimStart('、', '。', ' ', '\t');
                }
                else
                {
                    // Try matching post-conditions (e.g. X is equal to...)
                    var condMatch = registry.ConditionListRegistry.Match(t.AsMemory());
                    if (condMatch != null)
                    {
                        var condList = condMatch.Translate(registry);
                        var postConds = condList.Where(c => c.Type == ConditionType.PostCondition).ToList();
                        if (postConds.Count > 0)
                        {
                            abilityParts.AddRange(postConds.Select(c => c.ConditionText));
                            remainingText = t[condMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
                            continue;
                        }
                    }
                    remainingText = t.Length > 1 ? t[1..] : "";
                }
            }

            if (abilityParts.Count > 0)
                translatedSentences.Add(AutoEffectToken.JoinAbilityParts(abilityParts));
        }

        var trimmedSentences = translatedSentences.Select(s => s.TrimEnd('.')).ToList();
        var abilityEnglish = string.Join(". ", trimmedSentences);
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            abilityEnglish = char.ToUpper(abilityEnglish[0]) + abilityEnglish[1..];
        }

        return new EventCardEffect
        {
            Labels = ["Brainstorm"],
            Abilities = allAbilities,
            AbilityText = abilityEnglish,
            EffectText = $"Brainstorm {abilityEnglish}."
        };
    }
}
