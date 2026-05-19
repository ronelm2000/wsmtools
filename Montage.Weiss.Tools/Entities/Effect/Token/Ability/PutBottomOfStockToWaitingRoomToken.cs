namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutBottomOfStockToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:相手の)?ストックの下から(\d+)枚を、?控え室に置(?:く|いてよい|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var displayCount = count.Replace("X", "X");
        var may = match.Value.Contains("てよい");
        var verb = may ? "you may put" : "put";
        var isOpponent = match.Value.Contains("相手の");
        return
        [
            new CardEffectAbility
            {
                AbilityText = displayCount == "1"
                    ? isOpponent
                        ? $"{verb} the bottom card of your opponent's stock to their waiting room"
                        : $"{verb} the bottom card of your stock to your waiting room"
                    : isOpponent
                        ? $"{verb} the bottom {displayCount} cards of your opponent's stock to their waiting room"
                        : $"{verb} the bottom {displayCount} cards of your stock to your waiting room"
            }
        ];
    }
}
