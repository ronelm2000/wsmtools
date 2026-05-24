namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches named-CX-in-CX-area conditions in continuative form.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのCX置場に「Bunny X 777」があり</c></para>
/// <para><b>Regex:</b> ^あなたのCX置場に「(?&lt;name&gt;.+?)」があり</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "name": CX card name (e.g., "Bunny X 777")</description></item>
/// </list>
/// <para><b>Output:</b> <c>"Bunny X 777" is in your CX area</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class CxAreaNamedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCX置場に「(?<name>.+?)」があり");
    public override IEnumerable<string> SampleMatches => ["あなたのCX置場に「★TESTNAME★」があり"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"\"{name}\" is in your CX area"
            }
        ];
    }
}
