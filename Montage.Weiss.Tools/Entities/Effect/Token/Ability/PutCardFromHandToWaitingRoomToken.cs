namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCardFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"手札(?:のキャラ)?を(\d+)枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        var noun = match.Value.Contains("のキャラ") ? "character" : "card";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} {(count == 1 ? noun : noun + "s")} from your hand into your waiting room"
            }
        ];
    }
}
