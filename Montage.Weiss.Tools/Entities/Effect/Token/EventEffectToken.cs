namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class EventEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^.+$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
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

            var sentenceAbilities = new List<CardEffectAbility>();
            var sentenceParts = new List<string>();
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
                translatedSentences.Add(string.Join(", ", sentenceParts));
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
