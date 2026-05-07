namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ClockToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたは自分のクロックの上から(\d+)枚までを");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put up to {count} card(s) from the top of your clock into your waiting room"
            }
        ];
    }
}
