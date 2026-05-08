namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class BrainstormEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^集中\s*(?<rest>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
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
