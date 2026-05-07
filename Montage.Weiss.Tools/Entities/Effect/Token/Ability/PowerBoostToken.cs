namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカードのパワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power"
            }
        ];
    }
}
