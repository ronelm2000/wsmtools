namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class YourCxWithTriggerIconTriggeredConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]のCXがトリガーした時");

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
                ConditionText = $"your CX with [{iconName}] in its trigger icon triggers"
            }
        ];
    }
}
