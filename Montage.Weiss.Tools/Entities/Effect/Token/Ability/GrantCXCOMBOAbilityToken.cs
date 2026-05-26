namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GrantCXCOMBOAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の能力を与える。『【自】【CXコンボ】［(.+?)］(.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups[1].Value;
        var innerText = match.Groups[2].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, "【自】" + innerText);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following ability. \"[{costText}] {nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
