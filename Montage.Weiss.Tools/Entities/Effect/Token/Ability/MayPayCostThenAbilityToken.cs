namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MayPayCostThenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたはコストを払ってよい。そうしたら、(?<effect>.+)$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var effectText = match.Groups["effect"].Value.Trim();
        var nestedAbilities = registry.EffectListRegistry.GetMatch(effectText)(registry);

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may pay the cost. If you do, {string.Join(", ", nestedAbilities.Select(a => a.AbilityText))}"
            }
        ];
    }
}
