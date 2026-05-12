namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StockCostWithPutCardToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)\s*このカードを控え室に置く$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put this card to your waiting room"
            }
        ];
    }
}
