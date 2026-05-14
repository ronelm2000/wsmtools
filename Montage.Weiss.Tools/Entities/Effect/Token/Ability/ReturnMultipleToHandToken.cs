namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnMultipleToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^残りのカードを控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the rest to your waiting room"
            }
        ];
    }
}
