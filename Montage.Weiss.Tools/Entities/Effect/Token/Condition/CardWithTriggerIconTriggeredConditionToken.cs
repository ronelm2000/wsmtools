namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "when a card with a specific trigger icon triggers" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのトリガーアイコンに[[soul.gif]]があるカードがトリガーした時</c></para>
/// <para><b>Regex:</b> ^あなたのトリガーアイコンに\[\[(?&lt;icon&gt;[^\]]+?)\]\]があるカードがトリガーした時</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description><c>icon</c>: Trigger icon filename (e.g., <c>soul.gif</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>a card with [SOUL] in its trigger icon triggers</c> (as When-type condition)</para>
/// <para><b>Variant:</b> Unlike <see cref="YourCxWithTriggerIconTriggeredConditionToken"/> which uses <c>が[[icon]]のCX</c>,
/// this token matches the <c>に[[icon]]があるカード</c> pattern for non-CX cards.</para>
/// </remarks>
internal class CardWithTriggerIconTriggeredConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのトリガーアイコンに\[\[(?<icon>[^\]]+?)\]\]があるカードがトリガーした時");

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
                ConditionText = $"a card with [{iconName}] in its trigger icon triggers"
            }
        ];
    }
}
