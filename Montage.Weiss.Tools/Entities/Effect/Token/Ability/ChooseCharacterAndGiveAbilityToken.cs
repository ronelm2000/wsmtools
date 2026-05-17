using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(自分の)?キャラを(\d+)枚選び、そのターン中、次の能力を与える。『(.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var nestedJapanese = match.Groups[2].Value;
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your characters, and that character gets the following ability until end of turn. \"{nestedEnglish}\""
            }
        ];
    }
}
