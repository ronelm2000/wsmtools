namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterBoostAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分のキャラを(\d+)枚選び、そのターン中、パワーを[＋\+](\d+)し、次の能力を与える。『(.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var power = int.Parse(match.Groups[2].Value);
        var nestedJapanese = match.Groups[3].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;
        nestedEnglish = nestedEnglish.TrimEnd('.');

        var countText = count == 1 ? "1" : $"{count}";
        var charText = count == 1 ? "character" : "characters";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText} of your characters, and that {charText} gets +{power} power and the following ability until end of turn. \"{nestedEnglish}\""
            }
        ];
    }
}
