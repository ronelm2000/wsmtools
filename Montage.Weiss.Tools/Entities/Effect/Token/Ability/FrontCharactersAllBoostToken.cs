namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class FrontCharactersAllBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの前のあなたの(?:《(.+?)》の)?(?:(レベル(\d+)以上の)キャラすべて|キャラすべて)に、パワーを＋(\d+)。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Success ? match.Groups[1].Value : null;
        var level = match.Groups[3].Success ? match.Groups[3].Value : null;
        var power = match.Groups[4].Value;
        var prefix = trait is not null
            ? $"<<{trait}>>"
            : level is not null
                ? $"level {level} or higher"
                : "";
        var abilityText = $"All of your {prefix}{(prefix.Length > 0 ? " " : "")}characters in front of this card get +{power} power";
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}
