namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class WrTriggerIconExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの控え室にトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]のCXがあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconHelper.GetIconName(icon);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"a CX with [{iconName}] in its trigger icon is in your waiting room"
            }
        ];
    }
}
