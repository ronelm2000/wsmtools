namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DuringTurnPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのターン中、このカードのパワーを＋(\d+)\.");

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
