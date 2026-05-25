namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "there is a character with a name fragment under this card's marker" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのマーカーにカード名に「マリ」を含むキャラがあるなら</c></para>
/// <para><b>Regex:</b> ^このカードのマーカーにカード名に「(.+?)」を含むキャラがあるなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name fragment</description></item>
/// </list>
/// <para><b>Output:</b> <c>If there is a character with "name" in its card name under this card's marker</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class MarkerNamedCharacterExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードのマーカーにカード名に「(.+?)」を含むキャラがあるなら");
    public override IEnumerable<string> SampleMatches => ["このカードのマーカーにカード名に「マリ」を含むキャラがあるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there is a character with \"{name}\" in its card name under this card's marker"
            }
        ];
    }
}
