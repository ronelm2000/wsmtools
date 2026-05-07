namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutInHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"手札に加え");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put it into your hand"
            }
        ];
    }
}
