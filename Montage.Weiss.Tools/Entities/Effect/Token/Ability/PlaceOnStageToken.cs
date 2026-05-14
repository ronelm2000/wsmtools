namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PlaceOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^舞台の好きな枠に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put it on any position on your stage"
            }
        ];
    }
}
