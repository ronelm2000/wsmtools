namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxWithTriggerIconInCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
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

    public override Regex Matcher => new(@"^あなたのCX置場に(?:トリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]の)?CXがあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var iconGroup = match.Groups["icon"];
        if (iconGroup.Success)
        {
            var icon = iconGroup.Value;
            var iconName = TriggerIconNames.TryGetValue(icon, out var name) ? name : icon;
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = $"If a CX with [{iconName}] in its trigger icon is in your CX area"
                }
            ];
        }
        else
        {
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = "If a CX is in your CX area"
                }
            ];
        }
    }
}
