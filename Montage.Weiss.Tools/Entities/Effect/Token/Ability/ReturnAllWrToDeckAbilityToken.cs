namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReturnAllWrToDeckAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の控え室のカードすべてを、山札に戻し、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "return all cards from your waiting room to your deck, and shuffle your deck"
            }
        ];
    }
}
