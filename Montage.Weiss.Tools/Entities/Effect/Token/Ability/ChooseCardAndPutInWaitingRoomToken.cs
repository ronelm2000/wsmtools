namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCardAndPutInWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたは自分の手札を(\d+)枚選び、控え室に置(?:き|く)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} card from your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "put it into your waiting room"
            }
        ];
    }
}
