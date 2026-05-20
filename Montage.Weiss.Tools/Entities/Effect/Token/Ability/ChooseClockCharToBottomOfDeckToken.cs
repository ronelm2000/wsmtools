namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseClockCharToBottomOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分のクロック置場の《(.+?)》のキャラを(\d+)枚選び、山札の下に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        var countText = count == 1 ? $"1 <<{trait}>> character" : $"{count} <<{trait}>> characters";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {countText} in your clock"
            },
            new CardEffectAbility
            {
                AbilityText = "put it at the bottom of your deck"
            }
        ];
    }
}
