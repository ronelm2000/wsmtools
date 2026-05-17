namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCannotPlayEventsToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<OpponentCannotPlayEventsToken>();

    public override Regex Matcher => new(@"^相手はイベントを手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        Log.Debug("OpponentCannotPlayEventsToken: matched input='{Input}', success={Success}", span.ToString(), match.Success);
        return
        [
            new CardEffectAbility
            {
                AbilityText = "your opponent cannot play events from their hand"
            }
        ];
    }
}
