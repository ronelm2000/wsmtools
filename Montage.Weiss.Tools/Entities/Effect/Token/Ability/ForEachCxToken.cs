namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "for each CX among those cards" per-CX iteration clauses.
/// Handles the sub-action pattern <c>次の行動を行う。『...』</c> by translating the inner text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>それらのカードのCX1枚につき、次の行動を行う。『あなたは自分の控え室の...』</c></para>
/// <para><b>Regex:</b> ^それらのカードのCX1枚につき、?(?&lt;remaining&gt;.+)$</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>remaining: Text after the per-CX prefix, including any sub-action block</description></item>
/// </list>
/// <para><b>Output:</b> <c>For each CX revealed among those cards, ...</c></para>
/// </remarks>
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

        // Handle "perform the following action" sub-ability pattern: 次の行動を行う。『...』
        var actionMatch = Regex.Match(remainingText, @"^次の行動を行う。『(.+)』");
        if (actionMatch.Success)
        {
            var inner = actionMatch.Groups[1].Value;
            var innerResult = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, inner);
            if (innerResult != null)
            {
                result.Add(new CardEffectAbility { AbilityText = innerResult });
            }
            return result;
        }

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
