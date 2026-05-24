namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllCenterStageExceptThisCardGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカード以外のあなたの前列のキャラすべてに、そのターン中、次の能力を与える。『(.+)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["このカード以外のあなたの前列のキャラすべてに、そのターン中、次の能力を与える。『【永】 このカードは他の枠に動かせない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"all of your characters in your center stage except this card get the following ability until end of turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
