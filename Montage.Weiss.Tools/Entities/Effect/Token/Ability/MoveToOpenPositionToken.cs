namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MoveToOpenPositionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:このカードを)?前列の(?:キャラのいない|のキャラのいない他の)枠に動か(?:してよい|す)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var text = match.Value;
        var may = text.Contains("てよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? "you may move this card to an open position of your center stage"
                    : "move this card to an open position of your center stage"
            }
        ];
    }
}
