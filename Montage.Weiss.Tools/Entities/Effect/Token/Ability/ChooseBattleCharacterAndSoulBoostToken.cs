namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseBattleCharacterAndSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^バトル中のキャラを(\d+)枚選び、そのターン中、ソウルを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var soul = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} character in battle"
            },
            new CardEffectAbility
            {
                AbilityText = count == 1 ? $"that character gets +{soul} soul until end of turn" : $"those characters get +{soul} soul until end of turn"
            }
        ];
    }
}
