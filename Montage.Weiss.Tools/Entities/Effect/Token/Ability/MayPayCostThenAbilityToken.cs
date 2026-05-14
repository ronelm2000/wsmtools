namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MayPayCostThenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはコストを払ってよい。そうたら、(?<effect>.+)$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var effectText = match.Groups["effect"].Value.Trim();
        var allAbilities = new List<CardEffectAbility>();
        var abilityParts = new List<string>();
        var remainingText = effectText;

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
                allAbilities.AddRange(abilList);
                abilityParts.AddRange(abilList.Select(a => a.AbilityText));
                remainingText = trimmed[consumed..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                remainingText = trimmed.Length > 1 ? trimmed[1..] : "";
            }
        }

        var joined = AutoEffectToken.JoinAbilityParts(abilityParts);
        if (joined.Length > 0)
            joined = char.ToLower(joined[0]) + joined[1..];
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may pay the cost. If you do, {joined}"
            }
        ];
    }
}
