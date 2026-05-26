namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class LookAtTopDeckAndChoosePositionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?山札を上から(\d+)枚見て、山札の上か下か控え室に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の山札を上から1枚見て、山札の上か下か控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"look at the top {count} card of your deck, and put that card on the top of your deck, the bottom of your deck, or into your waiting room"
            }
        ];
    }
}
