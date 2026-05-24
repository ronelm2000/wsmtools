namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if a named CX is in your CX area" conditions.
/// Supports both <c>CX置場に「…」があり</c> and <c>あなたのCX置場に「…」があるなら</c> variants.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのCX置場に「A.C.P.U.!FREEZE!」があるなら</c></para>
/// <para><b>Regex:</b> ^(?:あなたの)?CX置場に「(?&lt;name&gt;.+?)」があ(?:り|るなら)</para>
/// <para><b>Capture:</b></para>
/// <list type="bullet">
///   <item><description>name: The CX card name (e.g., "A.C.P.U.!FREEZE!")</description></item>
/// </list>
/// <para><b>Output:</b> <c>If, "A.C.P.U.!FREEZE!" is in your CX area</c></para>
/// </remarks>
internal class CxNamedInCxAreaConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:あなたの)?CX置場に「(?<name>.+?)」があ(?:り|るなら)");
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
