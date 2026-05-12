namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TraitCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラが(\d+)枚以上(?:なら|で)");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If you have {count} or more other <<{trait}>> characters"
            }
        ];
    }
}
