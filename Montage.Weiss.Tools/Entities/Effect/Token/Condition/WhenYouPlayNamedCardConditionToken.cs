namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "when you play a named card" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたが「カード名」をプレイした時</c></para>
/// <para><b>Regex:</b> ^あなたが「(.+?)」をプレイした時</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name</description></item>
/// </list>
/// <para><b>Output:</b> <c>When you play "name"</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class WhenYouPlayNamedCardConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたが「(.+?)」をプレイした時");
    public override IEnumerable<string> SampleMatches => ["あなたが「カード名」をプレイした時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"you play \"{name}\""
            }
        ];
    }
}
