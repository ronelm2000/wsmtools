namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TraitCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups["trait"].Value;
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If you have {count} or more <<{trait}>> characters"
            }
        ];
    }
}
