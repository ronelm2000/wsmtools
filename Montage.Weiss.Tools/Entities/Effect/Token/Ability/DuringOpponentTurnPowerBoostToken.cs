namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DuringOpponentTurnPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のターン中、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"during your opponent's turn, this card gets +{power} power"
            }
        ];
    }
}
