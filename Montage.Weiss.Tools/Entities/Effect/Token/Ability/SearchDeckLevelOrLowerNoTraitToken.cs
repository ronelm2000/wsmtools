namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckLevelOrLowerNoTraitToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札を見てレベル(?<level>\d+)以下のキャラを(?<count>\d+)枚まで選んで相手に見せ、手札に加え、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups["level"].Value;
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} level {level} or lower character, reveal it to your opponent, put it to your hand, and shuffle your deck"
            }
        ];
    }
}
