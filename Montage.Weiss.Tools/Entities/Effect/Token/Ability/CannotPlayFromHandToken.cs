namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CannotPlayFromHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:このカードは)?手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card cannot be played in your hand"
            }
        ];
    }
}
