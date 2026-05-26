namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseBattleCharPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^バトル中のキャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = count == 1
                    ? $"choose 1 of your characters in battle, and that character gets +{power} power until end of turn"
                    : $"choose {count} of your characters in battle, and those characters get +{power} power until end of turn"
            }
        ];
    }
}
