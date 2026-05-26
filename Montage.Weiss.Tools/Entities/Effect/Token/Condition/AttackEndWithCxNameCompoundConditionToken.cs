namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches compound attack-end + CX-name + trait-count + facing-reverse condition clause.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのアタックの終わりに、CX置場に「ラッキーアクシデント」があり、他のあなたの《サマポケ》のキャラが2枚以上で、このカードの正面のキャラがいないか【リバース】しているなら</c></para>
/// <para><b>Regex:</b> ^このカードのアタックの終わりに、CX置場に「(?&lt;cxName&gt;.+?)」があり、他のあなたの《(?&lt;trait&gt;.+?)》のキャラが(?&lt;count&gt;\d+)枚以上で、このカードの正面のキャラがいないか【リバース】しているなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>cxName: CX card name</description></item>
///   <item><description>trait: Trait name</description></item>
///   <item><description>count: Trait character count threshold</description></item>
/// </list>
/// <para><b>Output (conditions):</b></para>
/// <list type="bullet">
///   <item><description>Condition 1 (At): the end of this card's attack</description></item>
///   <item><description>Condition 2 (If): "{cxName}" is in your CX area</description></item>
///   <item><description>Condition 3 (If): there are {count} or more of your other &lt;&lt;{trait}&gt;&gt; characters</description></item>
///   <item><description>Condition 4 (If): there is no character facing this card or the character facing this card is [REVERSE]</description></item>
/// </list>
/// </remarks>
internal class AttackEndWithCxNameCompoundConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのアタックの終わりに、CX置場に「(?<cxName>.+?)」があり、他のあなたの《(?<trait>.+?)》のキャラが(?<count>\d+)枚以上で、このカードの正面のキャラがいないか【リバース】しているなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cxName = registry.MatchNameFragment(match.Groups["cxName"].Value);
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count = match.Groups["count"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.At,
                ConditionText = "the end of this card's attack"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"\"{cxName}\" is in your CX area"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there are {count} or more of your other <<{trait}>> characters"
            },
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is no character facing this card or the character facing this card is [REVERSE]"
            }
        ];
    }
}
