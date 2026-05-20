namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckForCxToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見てCXを(\d+)枚まで選んで相手に見せ、手札に加え、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} CX"
            },
            new CardEffectAbility
            {
                AbilityText = "reveal it to your opponent"
            },
            new CardEffectAbility
            {
                AbilityText = "put it to your hand"
            },
            new CardEffectAbility
            {
                AbilityText = "shuffle your deck"
            }
        ];
    }
}
