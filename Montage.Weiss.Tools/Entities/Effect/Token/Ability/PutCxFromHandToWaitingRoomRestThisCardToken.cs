namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCxFromHandToWaitingRoomRestThisCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)\s*手札のCXを(\d+)枚控え室に置き、このカードを【レスト】する$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"({cost}) Put {count} CX in your hand to your waiting room, and [REST] this card"
            }
        ];
    }
}
