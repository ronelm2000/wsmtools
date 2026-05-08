namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class EncoreToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"アンコール\s*［(?<cost>.+?)］");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var costText = match.Groups["cost"].Value;
        var costAbilities = registry.EffectListRegistry.GetMatch(costText)(registry);
        var costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText))
            .Replace("your waiting room", "the waiting room");

        return [new CardEffectAbility { AbilityText = $"Encore [{costEnglish}]" }];
    }
}
