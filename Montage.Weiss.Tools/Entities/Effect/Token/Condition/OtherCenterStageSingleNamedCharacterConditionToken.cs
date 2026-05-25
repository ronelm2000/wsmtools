namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "you have another named character in center stage" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの前列の「カード名」がいるなら</c></para>
/// <para><b>Regex:</b> ^他のあなたの前列の「(.+?)」がいるなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name</description></item>
/// </list>
/// <para><b>Output:</b> <c>If you have another "name" in your center stage</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class OtherCenterStageSingleNamedCharacterConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列の「(.+?)」がいるなら");
    public override IEnumerable<string> SampleMatches => ["他のあなたの前列の「カード名」がいるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have another \"{name}\" in your center stage"
            }
        ];
    }
}
