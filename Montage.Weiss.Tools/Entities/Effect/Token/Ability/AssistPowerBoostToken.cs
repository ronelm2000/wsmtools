namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AssistPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの前のあなたのキャラすべてに、パワーを＋(?:X|\d+)(?:。X はそのキャラのレベル×(\d+) に等しい)?(?:。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var multiplier = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var power = match.Groups[1].Success ? match.Groups[1].Value : "X";
        var hasX = power == "X";
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
