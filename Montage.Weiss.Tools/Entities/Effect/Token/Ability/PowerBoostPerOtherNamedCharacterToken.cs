namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "for each other named character, power boost" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの「伊吹萃香」1枚につき、このカードのパワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^他のあなたの「(.+?)」1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character name in 「」 (e.g., <c>伊吹萃香</c>)</description></item>
///   <item><description>Group 2: Power boost value (e.g., <c>2000</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>For each of your other "name", this card gets +Y power</c></para>
/// <para><b>Counterpart:</b> <see cref="PowerBoostPerTraitCharacterToken"/> for trait-based per-character boosts.</para>
/// </remarks>
internal class PowerBoostPerOtherNamedCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの「(.+?)」1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["他のあなたの「伊吹萃香」1枚につき、このカードのパワーを＋2000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"For each of your other \"{name}\", this card gets +{power} power"
            }
        ];
    }
}
