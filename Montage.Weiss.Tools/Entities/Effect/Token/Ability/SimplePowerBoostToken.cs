namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SimplePowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのパワー＋(\d+)\.");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power"
            }
        ];
    }
}
