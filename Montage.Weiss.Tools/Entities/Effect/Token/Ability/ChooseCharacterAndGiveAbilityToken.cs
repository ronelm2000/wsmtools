namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(自分の)?キャラを(\d+)枚選び、そのターン中、次の能力を与える。『(.+)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分のキャラを1枚選び、そのターン中、次の能力を与える。『【永】 このカードは【リバース】しない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var nestedJapanese = match.Groups[2].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {count} of your characters, and that character gets the following ability until end of turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
