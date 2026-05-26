namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GrantStandaloneFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の能力を与える。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nested = match.Groups["nested"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nested);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
