namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "that character gets +power and +soul" boost clauses with optional duration.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのキャラのパワーを＋5000し、ソウルを＋1。</c></para>
/// <para><b>Regex:</b> ^(?:(?&lt;duration&gt;そのターン中|次の相手のターンの終わりまで|このターン中)、)?そのキャラのパワーを[＋\+](?&lt;power&gt;\d+)し、ソウルを[＋\+](?&lt;soul&gt;\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>duration: Optional duration prefix</description></item>
///   <item><description>power: Power boost value</description></item>
///   <item><description>soul: Soul boost value</description></item>
/// </list>
/// <para><b>Output:</b> <c>that character gets +{power} power and +{soul} soul[duration]</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Additional duration patterns</para>
/// </remarks>
internal class ThatCharacterPowerAndSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|次の相手のターンの終わりまで|このターン中)、)?そのキャラのパワーを[＋\+](?<power>\d+)し、ソウルを[＋\+](?<soul>\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["そのキャラのパワーを＋5000し、ソウルを＋1。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups["power"].Value;
        var soul = match.Groups["soul"].Value;
        var duration = match.Groups["duration"].Success ? " until end of turn" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"that character gets +{power} power and +{soul} soul{duration}"
            }
        ];
    }
}
