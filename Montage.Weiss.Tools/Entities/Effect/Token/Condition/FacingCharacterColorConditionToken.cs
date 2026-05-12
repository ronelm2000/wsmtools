namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class FacingCharacterColorConditionToken : CardTextToken<List<CardEffectCondition>>
{
    private static readonly Dictionary<string, string> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["黄"] = "yellow",
        ["赤"] = "red",
        ["青"] = "blue",
        ["緑"] = "green"
    };

    public override Regex Matcher => new(@"^このカードの正面のキャラが(?<color>.+?)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var color = match.Groups["color"].Value;
        var colorName = ColorMap.TryGetValue(color, out var name) ? name : color;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"the character facing this card is {colorName}"
            }
        ];
    }
}
