namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AssistPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカードの前のあなたのキャラすべてに、パワーを＋(Ｘ|\d+)(?:。Ｘはそのキャラのレベル×(\d+)に等しい。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        var multiplier = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var hasX = power == "Ｘ";
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasX
                    ? $"All of your characters in front of this card get +X power. X is equal to that character's level x{multiplier}"
                    : $"All of your characters in front of this card get +{power} power"
            }
        ];
    }
}
