namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ThoseCardsTriggerIconConditionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^それらのカードのトリガーアイコンが(?<icon>\[\[.+?\]\])の(?:(?<cxcount>CX(?:\d+)?枚につき)、(?:次の行動を行う。『(?<inner>.+)』)?)?(?<remaining>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var iconMatch = match.Groups["icon"].Value;
        var remaining = match.Groups["remaining"].Value.Trim();
        var cxCount = match.Groups["cxcount"].Success ? match.Groups["cxcount"].Value : null;
        var innerAction = match.Groups["inner"].Success ? match.Groups["inner"].Value.Trim() : null;

        // Extract the icon name from [[iconName]]
        var iconName = iconMatch.Replace("[", "").Replace("]", "").Replace(".gif", "").ToUpperInvariant();
        string iconText = $"[{iconName}]";

        // Create the "For each CX" phrase
        var cxPhrase = $"For each CX with {iconText} in its trigger icon revealed among those cards";

        // Handle sub-action pattern: 次の行動を行う。『...』
        if (innerAction != null)
        {
            var innerResult = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, innerAction);
            if (innerResult != null)
            {
                return
                [
                    new CardEffectAbility
                    {
                        AbilityText = $"{cxPhrase}, {innerResult}"
                    }
                ];
            }
        }

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
                    : $"{cxPhrase}, {abilityEnglish}"
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

            var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (abilMatch != null)
            {
                var abilList = abilMatch.Translate(registry);
                result.AddRange(abilList);
                remainingText = trimmed[abilMatch.Match.Length..].TrimStart('、', '。', ' ', '\t');
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
        if (parts.Count == 0) return "";
        if (parts.Count == 1) return parts[0];
        var allButLast = string.Join(", ", parts.Take(parts.Count - 1));
        return $"{allButLast}, and {parts[^1]}";
    }
}
