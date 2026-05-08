namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class EventEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^.+$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var input = match.Value;
        var sentences = input.Split('。', StringSplitOptions.RemoveEmptyEntries);
        var translatedSentences = new List<string>();
        var allAbilities = new List<CardEffectAbility>();

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            try
            {
                var sentenceAbilities = registry.EffectListRegistry.GetMatch(trimmed)(registry);
                allAbilities.AddRange(sentenceAbilities);
                translatedSentences.Add(string.Join(", ", sentenceAbilities.Select(a => a.AbilityText)));
            }
            catch (NotImplementedException)
            {
                translatedSentences.Add(trimmed);
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
