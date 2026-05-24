namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterBoostAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分のキャラを(\d+)枚選び、そのターン中、パワーを[＋\+](\d+)し、次の能力を与える。『(.+)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分のキャラを1枚選び、そのターン中、パワーを＋2000し、次の能力を与える。『【永】 このカードは【リバース】しない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var power = int.Parse(match.Groups[2].Value);
        var nestedJapanese = match.Groups[3].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        var countText = count == 1 ? "1" : $"{count}";
        var charText = count == 1 ? "character" : "characters";

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {countText} of your characters, and that {charText} gets +{power} power and the following ability until end of turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
