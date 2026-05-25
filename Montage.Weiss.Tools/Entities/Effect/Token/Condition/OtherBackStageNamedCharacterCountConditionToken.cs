namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "you have N or more other named characters in back stage" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの後列の「カード名」が2枚以上で、</c></para>
/// <para><b>Regex:</b> ^他のあなたの後列の「(.+?)」が(\d+)枚以上(?:で、)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name</description></item>
///   <item><description>Group 2: Count threshold</description></item>
/// </list>
/// <para><b>Output:</b> <c>If you have {N} or more other "name" in your back stage</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class OtherBackStageNamedCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの後列の「(.+?)」が(\d+)枚以上(?:で、)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの後列の「カード名」が2枚以上で、"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have {count} or more other \"{name}\" in your back stage"
            }
        ];
    }
}
