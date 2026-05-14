namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TriggerCheckRevealsCxWithIconConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのトリガーチェックでトリガーアイコンが\[\[(.+?)\]\]のCXがでた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.When,ConditionText = $"When this card's trigger check reveals a CX with [{icon.ToUpper()}] in its trigger icon"
            }
        ];
    }
}
