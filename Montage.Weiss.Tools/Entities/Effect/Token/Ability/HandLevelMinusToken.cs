namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class HandLevelMinusToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの手札の(?:(?:このカード)|「(?<name>.+?)」)のレベルを－(\d+).?$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var name = match.Groups["name"].Success ? match.Groups["name"].Value : null;
        var level = match.Groups[2].Value;
        
        // Clean up nested quotes for proper English formatting
        if (!string.IsNullOrEmpty(name))
        {
            // Remove trailing double quote if present
            name = name.TrimEnd('"');
            // Replace triple quotes with double quotes for proper formatting
            name = name.Replace("\"\"\"", "\"\"");
        }
        
        var abilityText = !string.IsNullOrEmpty(name)
            ? $"your \"{name}\" gets -{level} level while in your hand"
            : $"this card gets -{level} level while in your hand";
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}
