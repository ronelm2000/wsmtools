namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterAndBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのキャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        var power = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your characters, and that character gets +{power} power until end of turn"
            }
        ];
    }
}
