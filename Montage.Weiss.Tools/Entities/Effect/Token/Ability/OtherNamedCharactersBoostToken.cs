namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "all other characters with a name fragment get +power" boost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのカード名に「マリ」を含むキャラすべてに、パワーを＋1000。</c></para>
/// <para><b>Regex:</b> ^他のあなたのカード名に「(.+?)」を含むキャラすべてに、パワーを[＋\+](\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Name fragment (e.g., "マリ")</description></item>
///   <item><description>Group 2: Power boost value</description></item>
/// </list>
/// <para><b>Output:</b> <c>All your other characters with "name" in their card name get +{power} power</c></para>
/// </remarks>
internal class OtherNamedCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたのカード名に「(.+?)」を含むキャラすべてに、パワーを[＋\+](\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたのカード名に「マリ」を含むキャラすべてに、パワーを＋1000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All your other characters with \"{name}\" in their card name get +{power} power"
            }
        ];
    }
}
