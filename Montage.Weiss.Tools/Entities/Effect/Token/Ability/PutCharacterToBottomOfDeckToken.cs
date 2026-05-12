namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCharacterToBottomOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"そのキャラを山札の下に置いてよい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put that character to the bottom of your opponent's deck"
            }
        ];
    }
}
