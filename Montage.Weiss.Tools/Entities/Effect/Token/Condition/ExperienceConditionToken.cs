namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ExperienceConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^経験\s*あなたのレベル置場に、「(?<c1>.+?)」と「(?<c2>.+?)」があるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var c1 = match.Groups["c1"].Value;
        var c2 = match.Groups["c2"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If \"{c1}\" and \"{c2}\" are in your level"
            }
        ];
    }
}
