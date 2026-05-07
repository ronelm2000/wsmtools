namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCardFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"手札を(\d+)枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} card(s) from your hand into your waiting room"
            }
        ];
    }
}
