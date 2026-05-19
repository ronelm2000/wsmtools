namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches CX-area CX-with-trigger-icon conditions in continuative form.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのCX置場にトリガーアイコンが[[shot.gif]]のCXがあり</c></para>
/// <para><b>Regex:</b> ^あなたのCX置場にトリガーアイコンが\[\[(.+?)\]\]のCXがあり</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trigger icon filename (e.g., "shot.gif")</description></item>
/// </list>
/// <para><b>Output:</b> <c>a CX with [SHOT] in its trigger icon is in your CX area</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class CxAreaCxWithIconConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCX置場にトリガーアイコンが\[\[(.+?)\]\]のCXがあり");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups[1].Value;
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
}
