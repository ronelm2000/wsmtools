namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class EventEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^.+$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var input = match.Value;
        // Protect 。 inside 『』 blocks from being used as split points
        var protectedInput = Regex.Replace(input, @"『[^』]+』", m => m.Value.Replace("。", "\0"));
        var sentences = protectedInput.Split('。', StringSplitOptions.RemoveEmptyEntries);
        var translatedSentences = new List<string>();
        var allAbilities = new List<CardEffectAbility>();

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim().Replace("\0", "。");
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var conditions = new List<CardEffectCondition>();
            var sentenceAbilities = new List<CardEffectAbility>();
            var sentenceParts = new List<string>();
            var remainingText = trimmed;

            // Extract sentence-level pre-conditions first (e.g., "このカードは、あなたの《NIKKE》のキャラがいないなら")
            while (true)
            {
                var t = remainingText.TrimStart();
                if (t.Length == 0)
                    break;

                if (registry.ConditionListRegistry.TryMatchAtStart(t, out var condFunc, out var consumed) && condFunc != null)
                {
                    var condList = condFunc(registry);
                    conditions.AddRange(condList);
                    sentenceParts.AddRange(condList.Select(c => c.ConditionText));
                    remainingText = t[consumed..].TrimStart('、', '。', ' ', '\t');
                }
                else
                {
                    break;
                }
            }

            // Then parse remaining text for ability tokens
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
                    sentenceAbilities.AddRange(abilList);
                    sentenceParts.AddRange(abilList.Select(a => a.AbilityText));
                    remainingText = t[consumed..].TrimStart('、', '。', ' ', '\t');
                }
                else
                {
                    break;
                }
            }

            allAbilities.AddRange(sentenceAbilities);
            if (sentenceParts.Count > 0)
            {
                var joined = string.Join(", ", sentenceParts);
                if (translatedSentences.Count > 0)
                    joined = char.ToUpper(joined[0]) + joined[1..];
                translatedSentences.Add(joined);
            }
        }

        var abilityEnglish = string.Join(". ", translatedSentences);
        if (!string.IsNullOrEmpty(abilityEnglish))
        {
            abilityEnglish = char.ToUpper(abilityEnglish[0]) + abilityEnglish[1..];
        }

        var effectText = abilityEnglish;
        if (!string.IsNullOrEmpty(effectText) && !effectText.EndsWith('.'))
            effectText += ".";

        return new EventCardEffect
        {
            Labels = [],
            EffectText = effectText,
            AbilityText = abilityEnglish,
            Abilities = allAbilities
        };
    }
}
