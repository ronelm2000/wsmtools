namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "no character with given trait exists" conditional clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの《★TESTTRAIT★》のキャラがいないなら</c> or <c>他の《武器》のキャラがいないなら</c></para>
/// <para><b>Regex:</b> ^(?:このカードは、)?(?:他の)?(?:あなた(?:に|の))?《(.+?)》のキャラがいないなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name</description></item>
/// </list>
/// <para><b>Output (other):</b> <c>you have no other &lt;&lt;{trait}&gt;&gt; character</c></para>
/// <para><b>Output (non-other):</b> <c>you do not have a &lt;&lt;{trait}&gt;&gt; character</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class NoTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:このカードは、)?(?:他の)?(?:あなた(?:に|の))?《(.+?)》のキャラがいないなら");
    public override IEnumerable<string> SampleMatches =>
    [
        "あなたの《★TESTTRAIT★》のキャラがいないなら",
        "他の《武器》のキャラがいないなら"
    ];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var isOther = match.Value.StartsWith("他の");
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = isOther ? $"you have no other <<{trait}>> character" : $"you do not have a <<{trait}>> character"
            }
        ];
    }
}
