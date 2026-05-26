namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWrPutToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚選び、思い出にしてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の控え室の「カード名」を1枚選び、思い出にしてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose a card named \"{name}\" in your waiting room, and put it into your memory"
            }
        ];
    }
}
