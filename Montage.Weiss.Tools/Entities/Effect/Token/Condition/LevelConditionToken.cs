namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class LevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:このカードは、)?あなたのレベル置場に、黄のカードと赤のカードと青のカードが(?<state>ある|ない)なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var state = match.Groups["state"].Value;
        var conditionText = state == "ある"
            ? "If your level has a yellow card, a red card and a blue card"
            : "If you do not have a yellow card, a red card and a blue card in your level";
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = conditionText
            }
        ];
    }
}
