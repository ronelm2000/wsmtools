namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterAndBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたの|自分の)?キャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var power = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your characters, and it gets +{power} power until end of turn"
            }
        ];
    }
}
