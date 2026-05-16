namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCannotPlayEventsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手はイベントを手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "opponent cannot play events from their hand"
            }
        ];
    }
}
