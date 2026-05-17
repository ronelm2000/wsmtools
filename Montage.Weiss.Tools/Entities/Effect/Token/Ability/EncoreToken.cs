namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class EncoreToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^アンコール\s*［(?<cost>.+?)］");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups["cost"].Value;
        var costAbilities = registry.EffectListRegistry.GetMatch(costText.AsMemory())(registry);
        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        if (!string.IsNullOrEmpty(costEnglish))
            costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];
        costEnglish = costEnglish.Replace("to your waiting room", "to the waiting room", StringComparison.Ordinal);

        return [new CardEffectAbility { AbilityText = $"Encore [{costEnglish}]" }];
    }
}
