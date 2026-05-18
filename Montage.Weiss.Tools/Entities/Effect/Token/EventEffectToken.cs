namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class EventEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<EventEffectToken>();

    public override Regex Matcher => new(@"^(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        // Match: 経験 共鳴 
        var labels = registry.MatchLabels(match.Value);
        var input = match.Groups["mainText"].Value;
        // Protect 。 inside or before 『』 blocks from being used as split points
        var protectedInput = Regex.Replace(input, @"。?『[^』]+』", m => m.Value.Replace("。", "\0"));
        var sentences = protectedInput.Split('。', StringSplitOptions.RemoveEmptyEntries);
        var translatedSentences = new List<string>();
        var allAbilities = new List<CardEffectAbility>();

        Log.Debug("EventEffectToken: raw=[{raw}] labels=[{labels}] input=[{input}] protected=[{protected}] sentences.count={count}",
            span.ToString(), (object)labels.Length, input, protectedInput, sentences.Length);

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim().Replace("\0", "。");
            if (string.IsNullOrEmpty(trimmed))
                continue;

            Log.Debug("EventEffectToken: processing sentence=[{sentence}]", trimmed);

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
                    Log.Debug("EventEffectToken: matched condition at start consumed={consumed} text=[{text}]", consumed, t[..consumed]);
                    var condList = condFunc(registry);
                    conditions.AddRange(condList);
                    sentenceParts.AddRange(condList.Select(c => c.ConditionText));
                    remainingText = t[consumed..].TrimStart('、', '。', ' ', '\t');
                }
                else
                {
                    Log.Debug("EventEffectToken: no condition match at start, remaining=[{remaining}]", t);
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
                        Log.Debug("EventEffectToken: skipped {n} chars before ability match, remaining=[{remaining}]", matchIndex, t[..50]);
                        remainingText = t[matchIndex..];
                        continue;
                    }
                    Log.Debug("EventEffectToken: matched ability consumed={consumed} match=[{match}]", consumed, t[..consumed]);
                    var abilList = abilFunc(registry);
                    sentenceAbilities.AddRange(abilList);
                    sentenceParts.AddRange(abilList.Select(a => a.AbilityText));
                    remainingText = t[consumed..].TrimStart('、', '。', ' ', '\t');
                }
                else
                {
                    Log.Debug("EventEffectToken: no ability match, remaining=[{remaining}]", t);
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
                Log.Debug("EventEffectToken: sentence done -> [{joined}] (sentenceParts={count})", joined, sentenceParts.Count);
            }
            else
            {
                Log.Debug("EventEffectToken: sentence produced no parts -> skipped");
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

        Log.Debug("EventEffectToken: final effectText=[{effectText}] abilityEnglish=[{ability}] labels={labels}", effectText, abilityEnglish, labels);

        return new EventCardEffect
        {
            Labels = labels,
            EffectText = effectText,
            AbilityText = abilityEnglish,
            Abilities = allAbilities
        };
    }
}
