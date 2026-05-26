namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class YourCxWithTriggerIconPlacedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]のCXがCX置場に置かれた時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconHelper.GetIconName(icon);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"your CX with [{iconName}] in its trigger icon is placed in your CX area"
            }
        ];
    }
}
