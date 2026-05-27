namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCharPowerMinus2000Token : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のキャラを1枚選び、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 of your opponent's characters, and that character gets -{power} power until end of turn"
            }
        ];
    }
}
