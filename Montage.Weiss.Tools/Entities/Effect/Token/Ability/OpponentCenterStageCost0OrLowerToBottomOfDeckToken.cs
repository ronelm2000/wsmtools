namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCenterStageCost0OrLowerToBottomOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のコスト(\d+)以下のキャラを(\d+)枚選び、山札の下に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        var countText = count == 1 ? $"a cost {cost} or lower character" : $"{count} cost {cost} or lower characters";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {count} cost {cost} or lower character in your opponent's center stage"
            },
            new CardEffectAbility
            {
                AbilityText = "put it at the bottom of their deck"
            }
        ];
    }
}
