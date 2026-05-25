namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "other character with name A or name B is frontal attacked" when-condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのカード名に「マリ」か「ルゥ」を含むキャラがフロントアタックされた時</c></para>
/// <para><b>Regex:</b> ^他のあなたのカード名に「(.+?)」か「(.+?)」を含むキャラがフロントアタックされた時</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: First card name fragment</description></item>
///   <item><description>Group 2: Second card name fragment</description></item>
/// </list>
/// <para><b>Output:</b> <c>When your other character with "name1" or "name2" in its card name is frontal attacked</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class OtherNamedCharacterFrontalAttackedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのカード名に「(.+?)」か「(.+?)」を含むキャラがフロントアタックされた時");
    public override IEnumerable<string> SampleMatches => ["他のあなたのカード名に「マリ」か「ルゥ」を含むキャラがフロントアタックされた時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name1 = registry.MatchNameFragment(match.Groups[1].Value);
        var name2 = registry.MatchNameFragment(match.Groups[2].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = $"your other character with \"{name1}\" or \"{name2}\" in its card name is frontal attacked"
            }
        ];
    }
}
