namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ClockToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分のクロックの上から(\d+)枚までを、控え室に置き");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "You may put the top card of your clock to your waiting room"
            }
        ];
    }
}
