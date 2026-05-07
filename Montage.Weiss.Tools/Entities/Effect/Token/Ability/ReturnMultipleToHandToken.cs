namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnMultipleToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"残りのカードを控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put the rest into your waiting room"
            }
        ];
    }
}
