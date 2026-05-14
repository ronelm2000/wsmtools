namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ColorConditionIgnorePlayToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは、色条件を満たさずに手札からプレイできる。そうでないなら、手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card can be played in your hand without fulfilling color requirements. Otherwise, this card cannot be played in your hand"
            }
        ];
    }
}
