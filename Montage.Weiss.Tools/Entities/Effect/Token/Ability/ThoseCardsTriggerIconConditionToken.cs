namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ThoseCardsTriggerIconConditionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^それらのカードのトリガーアイコンが(?<icon>\[[^\]]+\])の(?<remaining>.+)$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var iconMatch = match.Groups["icon"].Value;
        var remaining = match.Groups["remaining"].Value.Trim();

        // Extract the icon name from [[iconName]]
        var iconMatchResult = System.Text.RegularExpressions.Regex.Match(iconMatch, @"\[\[(.+?)\]\]");
        string iconText = "[???]";
        if (iconMatchResult.Success && iconMatchResult.Groups[1].Success)
        {
            iconText = $"[{iconMatchResult.Groups[1].Value}]";
        }

        // Create the "For each CX" phrase
        var cxPhrase = $"For each CX with {iconText} in its trigger icon revealed among those cards";

        // Parse the remaining text
        var abilities = ParseTokenList(registry, remaining);
        var abilityEnglish = JoinAbilityParts(abilities.Select(a => a.AbilityText).ToList());

        // Combine into a single ability
        return
        [
            new CardEffectAbility
            {
                AbilityText = string.IsNullOrEmpty(abilityEnglish)
                    ? cxPhrase
                    : $"{cxPhrase} {abilityEnglish}"
            }
        ];
    }

    private static List<CardEffectAbility> ParseTokenList(ITokenRegistry registry, string text)
    {
        var result = new List<CardEffectAbility>();
        var remainingText = text;

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var trimmed = remainingText.TrimStart();
            if (trimmed.Length == 0)
                break;

            if (registry.EffectListRegistry.TryFindFirstMatch(trimmed, out var abilFunc, out var matchIndex, out var consumed) && abilFunc != null)
            {
                if (matchIndex > 0)
                {
                    remainingText = trimmed[matchIndex..];
                    continue;
                }
                var abilList = abilFunc(registry);
                result.AddRange(abilList);
                remainingText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                remainingText = trimmed.Length > 1 ? trimmed[1..] : "";
            }
        }

        return result;
    }

    private static string JoinAbilityParts(List<string> parts)
    {
        return string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }
}
