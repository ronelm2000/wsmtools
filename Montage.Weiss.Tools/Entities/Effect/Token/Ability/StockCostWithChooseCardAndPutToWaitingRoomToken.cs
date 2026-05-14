namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StockCostWithChooseCardAndPutToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)\s*手札(?:のキャラ)?を(\d+)枚選び、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[2].Value);
        var noun = match.Value.Contains("のキャラ") ? "character" : "card";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} {(count == 1 ? noun : noun + "s")} in your hand, and put it to your waiting room"
            }
        ];
    }
}
