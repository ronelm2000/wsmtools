namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseYourOtherCenterStageLevel0OrLowerCharToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは他の自分の前列のレベル(\d+)以下のキャラを(\d+)枚選び、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = count == 1
                    ? $"choose 1 of your other level {level} or lower characters in your center stage"
                    : $"choose {count} of your other level {level} or lower characters in your center stage"
            },
            new CardEffectAbility
            {
                AbilityText = count == 1 ? "put it to your waiting room" : "put them to your waiting room"
            }
        ];
    }
}
