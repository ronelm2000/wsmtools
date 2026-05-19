namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentCenterStageLevelXOrLowerCharToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のレベル(\d+)以下のキャラを(\d+)枚選び、控え室に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {count} level {level} or lower character in your opponent's center stage, and put it to their waiting room"
            }
        ];
    }
}
