namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches conditions where you do NOT have another specific named card.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの「"フィナーレ"ノワール」がいないなら</c></para>
/// <para><b>Regex:</b> ^他のあなたの「(?&lt;name&gt;.+?)」がいないなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "name": Card name (e.g., ""フィナーレ"ノワール")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you do not have another ""フィナーレ"ノワール"</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class AnotherSpecificCardNotExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<name>.+?)」がいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you do not have another \"{name}\""
            }
        ];
    }
}
