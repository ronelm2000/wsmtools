namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTriggerCxFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> TriggerIconNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["soul.gif"] = "SOUL",
        ["bounce.gif"] = "BOUNCE",
        ["shot.gif"] = "SHOT",
        ["choice.gif"] = "CHOICE",
        ["treasure.gif"] = "TREASURE",
        ["stock.gif"] = "POOL",
        ["standby.gif"] = "STANDBY",
        ["comeback.gif"] = "COMEBACK",
        ["salvage.gif"] = "COMEBACK",
        ["gate.gif"] = "GATE",
        ["draw.gif"] = "BOOK",
        ["discover.gif"] = "DISCOVER",
        ["chance.gif"] = "CHANCE",
        ["focus.gif"] = "FOCUS"
    };

    public override Regex Matcher => new(@"手札のトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]のCXを1枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconNames.TryGetValue(icon, out var name) ? name : icon;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 CX with [{iconName}] in its trigger icon in your hand to your waiting room"
            }
        ];
    }
}
