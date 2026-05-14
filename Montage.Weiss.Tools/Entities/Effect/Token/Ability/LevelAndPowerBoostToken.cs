namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class LevelAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのレベルを＋(\d+)し、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{level} level and +{power} power"
            }
        ];
    }
}
