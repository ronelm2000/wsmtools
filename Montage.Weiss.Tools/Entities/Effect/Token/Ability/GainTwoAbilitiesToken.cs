namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GainTwoAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の(\d+)つの能力を得る。『(?<a1>.+?)』『(?<a2>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["次の2つの能力を得る。『【自】能力1』『【自】能力2』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var a1 = match.Groups["a1"].Value;
        var a2 = match.Groups["a2"].Value;

        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, a1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, a2);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following {count} abilities. \"{nestedEffect1.EffectText}\" \"{nestedEffect2.EffectText}\"",
                NestedEffect = nestedEffect1,
                IsUnmatched = nestedEffect1.Abilities.Any(a => a.IsUnmatched) || nestedEffect2.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
