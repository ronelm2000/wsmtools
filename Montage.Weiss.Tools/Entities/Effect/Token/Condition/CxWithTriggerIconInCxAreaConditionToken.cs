namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CxWithTriggerIconInCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCX置場に(?:トリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]の)?CXがあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var iconGroup = match.Groups["icon"];
        if (iconGroup.Success)
        {
            var icon = iconGroup.Value;
            var iconName = TriggerIconHelper.GetIconName(icon);
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = $"a CX with [{iconName}] in its trigger icon is in your CX area"
                }
            ];
        }
        else
        {
            return
            [
                new CardEffectCondition
                {
                    Type = ConditionType.If,
                    ConditionText = "a CX is in your CX area"
                }
            ];
        }
    }
}
