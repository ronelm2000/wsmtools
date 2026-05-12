namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*\.$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[3].Success ? match.Groups[3].Value : match.Groups[4].Value;
        var pickCount = match.Groups[5].Value.Replace("Ｘ", "X");
        
        var fullMatch = match.Groups[0].Value;
        var revealIndex = fullMatch.IndexOf("相手に見せ", StringComparison.Ordinal);
        var additional = string.Empty;
        
        if (revealIndex >= 0)
        {
            var remaining = fullMatch.Substring(revealIndex + 6).Trim();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                // Split by Japanese comma and join with " and "
                var parts = remaining.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    additional = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        additional += " and " + parts[i];
                    }
                }
            }
        }
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {pickCount} <<{trait}>> character, reveal it to your opponent{additional}"
            }
        ];
    }
}

internal class SearchDeckWithTopLookToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を上から(.+?)枚まで見て、その中から《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = match.Groups[2].Value;
        var pickCount = match.Groups[3].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} cards, choose up to {pickCount} <<{trait}>> character from among them, reveal it to your opponent"
            }
        ];
    }
}
