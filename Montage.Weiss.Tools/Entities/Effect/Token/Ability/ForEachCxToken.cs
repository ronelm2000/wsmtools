namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ForEachCxToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^それらのカードのCX1枚につき、?(?<remaining>.+)$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var remaining = match.Groups["remaining"].Value.Trim();

        var cxPhrase = "For each CX revealed among those cards";

        var abilities = ParseTokenList(registry, remaining);
        var abilityEnglish = JoinAbilityParts(abilities.Select(a => a.AbilityText).ToList());

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
