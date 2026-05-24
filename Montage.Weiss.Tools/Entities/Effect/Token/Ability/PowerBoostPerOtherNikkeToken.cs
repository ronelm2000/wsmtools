namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Per other trait character" power boost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの《NIKKE》のキャラ 1 枚につき、このカードのパワーを＋3000。</c></para>
/// <para><b>Regex:</b> ^他のあなたの《(.+?)》のキャラ 1 枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
///   <item><description>Group 2: Power value (e.g., "3000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>This card gets +3000 power for each of your other &lt;&lt;trait&gt;&gt; characters</c></para>
/// </remarks>
internal class PowerBoostPerOtherNikkeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラ1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラ1枚につき、このカードのパワーを＋3000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = registry.MatchNameFragment(match.Groups[1].Value);
            var power = match.Groups[2].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"This card gets +{power} power for each of your other <<{trait}>> characters"
                }
            ];
        }
        return [];
    }
}
