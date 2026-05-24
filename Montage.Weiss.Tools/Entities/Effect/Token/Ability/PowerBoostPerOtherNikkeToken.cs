namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Per other NIKKE character" power boost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの《NIKKE》のキャラ 1 枚につき、このカードのパワーを＋3000。</c></para>
/// <para><b>Regex:</b> ^他のあなたの《NIKKE》のキャラ 1 枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power value (e.g., "3000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>This card gets +3000 power for each of your other &lt;&lt;NIKKE&gt;&gt; characters</c></para>
/// </remarks>
internal class PowerBoostPerOtherNikkeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《NIKKE》のキャラ1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《NIKKE》のキャラ1枚につき、このカードのパワーを＋3000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var power = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"This card gets +{power} power for each of your other <<{registry.MatchNameFragment("NIKKE")}>> characters"
                }
            ];
        }
        return [];
    }
}
