namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ShuffleDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札をシャッフルする");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "shuffle your deck"
            }
        ];
    }
}
