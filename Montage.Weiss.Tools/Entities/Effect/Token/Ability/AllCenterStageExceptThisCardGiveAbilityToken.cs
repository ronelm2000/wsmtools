namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllCenterStageExceptThisCardGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカード以外のあなたの前列のキャラすべてに、そのターン中、次の能力を与える。『(.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your characters in your center stage except this card get the following ability until end of turn. \"{nestedEnglish}\""
            }
        ];
    }
}
