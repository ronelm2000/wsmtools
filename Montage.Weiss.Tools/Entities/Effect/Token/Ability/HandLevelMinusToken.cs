namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class HandLevelMinusToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの手札の(?:(?:このカード)|「(.+?)」)のレベルを－(\d+).?$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var name = match.Groups[1].Success ? match.Groups[1].Value : null;
        var level = match.Groups[2].Value;
        var abilityText = name is not null
            ? $"""your ""{name}"" gets -{level} level while in your hand"""
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
