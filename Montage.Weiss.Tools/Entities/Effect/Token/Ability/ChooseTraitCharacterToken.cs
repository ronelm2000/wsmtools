namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"《(.+?)》のキャラを(Ｘ|\d+)枚まで選(?:び|んで)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} <<{trait}>> character from among them"
            }
        ];
    }
}
