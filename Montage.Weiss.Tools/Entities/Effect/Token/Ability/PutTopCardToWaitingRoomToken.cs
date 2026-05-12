namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutTopCardToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札の上から.*?控え室に置(?:いてよい|く|いて|き)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var may = match.Value.Contains("てよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? "you may put the top card of your deck to your waiting room"
                    : "put the top card of your deck to your waiting room"
            }
        ];
    }
}
