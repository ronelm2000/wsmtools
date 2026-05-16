namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class TurnOnceAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^この能力は1ターンにつき1回まで発動する(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This ability activates up to 1 time per turn"
            }
        ];
    }
}
