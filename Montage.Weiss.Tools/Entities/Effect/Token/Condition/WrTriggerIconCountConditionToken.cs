namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class WrTriggerIconCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの控え室のトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]のCXが(?<count>\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconHelper.GetIconName(icon);
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there are {count} or more CX with [{iconName}] in its trigger icon in your waiting room"
            }
        ];
    }
}
