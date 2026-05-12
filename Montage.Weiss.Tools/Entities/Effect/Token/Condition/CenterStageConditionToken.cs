namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CenterStageConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?<pos>前列(?:の中央の枠)?|舞台)にこのカードがい(?:るなら|て)");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var pos = match.Groups["pos"].Value;
        var conditionText = pos switch
        {
            "前列" => "this card is in your center stage",
            "前列の中央の枠" => "this card is in the middle position of your center stage",
            "舞台" => "this card is in the stage",
            _ => $"this card is in the {pos}"
        };
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = conditionText
            }
        ];
    }
}
