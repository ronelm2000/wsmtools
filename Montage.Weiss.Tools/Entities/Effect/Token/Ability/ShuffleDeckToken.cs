namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ShuffleDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札をシャッフルする");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "shuffle your deck"
            }
        ];
    }
}
