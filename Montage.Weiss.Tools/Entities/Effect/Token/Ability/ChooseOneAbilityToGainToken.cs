namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOneAbilityToGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?このカードは次の2つの能力のうちあなたが選んだ1つを得る。『(.+?)』『(.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var ability1 = match.Groups[1].Value;
        var ability2 = match.Groups[2].Value;
        var translated1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, ability1);
        var translated2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, ability2);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets one of the following two abilities of your choice. \"{translated1.EffectText}\" \"{translated2.EffectText}\"",
                NestedEffect = translated1,
                IsUnmatched = translated1.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
