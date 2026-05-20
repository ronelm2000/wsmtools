namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerAndSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのパワーを＋(\d+)し、ソウルを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        var soul = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and +{soul} soul"
            }
        ];
    }
}
