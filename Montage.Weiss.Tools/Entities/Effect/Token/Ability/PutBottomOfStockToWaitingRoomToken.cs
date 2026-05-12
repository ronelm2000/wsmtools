namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutBottomOfStockToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^ストックの下から(\d+) 枚を？控え室に置(?:く|いてよい|き)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups[1].Value;
        var displayCount = count.Replace("X", "X");
        var may = match.Value.Contains("てよい");
        var verb = may ? "you may put" : "put";
        return
        [
            new CardEffectAbility
            {
                AbilityText = displayCount == "1"
                    ? $"{verb} the bottom card of your stock to your waiting room"
                    : $"{verb} the bottom {displayCount} cards of your stock to your waiting room"
            }
        ];
    }
}
