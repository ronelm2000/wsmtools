namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckForNamedCardToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札を見てカード名に「(.+?)」を含むキャラを(\d+)枚まで選び、控え室に置き、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} character with \"{name}\" in its card name, put it into your waiting room, and shuffle your deck"
            }
        ];
    }
}
