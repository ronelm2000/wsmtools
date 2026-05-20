namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterAndSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?キャラを(\d+)枚選び、そのターン中、ソウルを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var soul = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your characters"
            },
            new CardEffectAbility
            {
                AbilityText = count == 1 ? $"that character gets +{soul} soul until end of turn" : $"those characters get +{soul} soul until end of turn"
            }
        ];
    }
}
