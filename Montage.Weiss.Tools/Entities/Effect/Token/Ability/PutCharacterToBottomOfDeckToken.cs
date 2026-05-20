namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCharacterToBottomOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのキャラを山札の下に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put that character at the bottom of your opponent's deck"
            }
        ];
    }
}
