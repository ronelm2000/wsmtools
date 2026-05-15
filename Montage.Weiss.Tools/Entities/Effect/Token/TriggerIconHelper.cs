using System.Collections.Frozen;

namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal static class TriggerIconHelper
{
    private static readonly FrozenDictionary<string, string> _triggerIconNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
    }.ToFrozenDictionary();

    public static string GetIconName(string gifName)
    {
        return _triggerIconNames.TryGetValue(gifName, out var name) ? name : gifName.Replace(".gif", "").ToUpperInvariant();
    }

    public static string GetRawIconName(string gifName)
    {
        return gifName.Replace(".gif", "").ToUpperInvariant();
    }
}
