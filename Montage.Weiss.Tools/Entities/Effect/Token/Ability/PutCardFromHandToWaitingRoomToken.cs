namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCardFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札(?:のキャラ)?を(\d+)枚控え室に置(?:いてよい|く|き)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var noun = match.Value.Contains("のキャラ") ? "character" : "card";
        var may = match.Value.Contains("てよい");
        var verb = may ? "Put" : "put";
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? $"you may put {count} {(count == 1 ? noun : noun + "s")} in your hand to your waiting room"
                    : $"{verb} {count} {(count == 1 ? noun : noun + "s")} in your hand to your waiting room"
            }
        ];
    }
}
