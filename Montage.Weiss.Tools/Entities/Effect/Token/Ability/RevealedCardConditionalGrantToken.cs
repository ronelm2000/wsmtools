namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RevealedCardConditionalGrantToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^公開したカードが「(?<card>[^」]+)」かレベル(?<level>\d+)以上のカードなら、次の相手のターンの終わりまで、このカードは次の能力を得る。『(?<abil>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["公開したカードが「★NAME★」かレベル1以上のカードなら、次の相手のターンの終わりまで、このカードは次の能力を得る。『【自】能力』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cardName = registry.MatchNameFragment(match.Groups["card"].Value);
        var level = match.Groups["level"].Value;
        var abil = match.Groups["abil"].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, abil);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"if the revealed card is \"{cardName}\" or a level {level} or higher card, this card gets the following ability until the end of your opponent's next turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched),
                Prefix = AbilityPrefix.AfterCannotBePlayed
            }
        ];
    }
}
