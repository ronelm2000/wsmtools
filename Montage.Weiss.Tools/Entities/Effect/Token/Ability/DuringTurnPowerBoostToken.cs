namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DuringTurnPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたのターン中、このカードのパワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"During your turn, this card gets +{power} power"
            }
        ];
    }
}
