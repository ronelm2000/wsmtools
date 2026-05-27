namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckForNamedCardPutOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?山札を見て「(.+?)」を(\d+)枚(?:まで)?選び、舞台の別々の枠に置き、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Search your deck for up to {count} \"{name}\", put them on different positions on your stage, and shuffle your deck"
            }
        ];
    }
}
