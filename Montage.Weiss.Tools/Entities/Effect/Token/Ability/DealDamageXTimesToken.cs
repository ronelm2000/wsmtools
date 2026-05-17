namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Deal damage X times" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手に 1 ダメージを 2 回与える。</c></para>
/// <para><b>Regex:</b> ^相手に 1 ダメージを (\d+) 回与える (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of times (e.g., "2")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Deal 1 damage to your opponent X times</c></para>
/// </remarks>
internal class DealDamageXTimesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に1ダメージを(\d+)回与える");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var times = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"Deal 1 damage to your opponent {times} times"
                }
            ];
        }
        return [];
    }
}
