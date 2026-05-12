namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MoveToOpenPositionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^前列のキャラのいない枠に動かし(?:てよい|く)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var text = match.Value;
        var may = text.Contains("てよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? "you may move this card to an open position of the center stage"
                    : "move this card to an open position of the center stage"
            }
        ];
    }
}
