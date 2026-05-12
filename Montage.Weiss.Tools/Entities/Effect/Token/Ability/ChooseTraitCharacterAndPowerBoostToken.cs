namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitCharacterAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の《(.+?)》のキャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        var power = int.Parse(match.Groups[3].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your <<{trait}>> characters, and it gets +{power} power until end of turn"
            }
        ];
    }
}
