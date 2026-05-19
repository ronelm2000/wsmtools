namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentCannotPlayEventsToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<OpponentCannotPlayEventsToken>();

    public override Regex Matcher => new(@"^相手はイベント(?:と『助太刀』)?を手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var hasBackup = match.Value.Contains("助太刀");
        Log.Debug("OpponentCannotPlayEventsToken: matched input='{Input}', hasBackup={HasBackup}", span.ToString(), hasBackup);
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasBackup
                    ? "your opponent cannot play events or \"Backup\" from their hand"
                    : "your opponent cannot play events from their hand"
            }
        ];
    }
}
