namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DuringTurnPowerBoostFromCIPToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのターン中、このカードのパワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power until end of turn"
            }
        ];
    }
}
