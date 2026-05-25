namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "a named card is in memory" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの思い出置場に「カード名」があるなら</c></para>
/// <para><b>Regex:</b> ^あなたの思い出置場に「(.+?)」があるなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name</description></item>
/// </list>
/// <para><b>Output:</b> <c>If "name" is in your memory</c></para>
/// <para><b>Type:</b> <c>ConditionType.LocationIf</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different memory area references</para>
/// </remarks>
internal class CardInMemoryNamedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたの思い出置場に「(.+?)」があるなら");
    public override IEnumerable<string> SampleMatches => ["あなたの思い出置場に「カード名」があるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.LocationIf,
                ConditionText = $"\"{name}\" is in your memory"
            }
        ];
    }
}
