namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "during your turn, this card gets +X power" clauses.
/// Unlike <see cref="PowerBoostWithDurationToken"/>, this token captures the duration inline in the regex
/// and does not use the external duration/pendingDuration mechanism.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたのターン中、このカードのパワーを＋5000。</c></para>
/// <para><b>Regex:</b> ^あなたのターン中、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power boost amount (e.g., "5000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>during your turn, this card gets +{power} power</c></para>
/// </remarks>
internal class DuringTurnPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのターン中、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたのターン中、このカードのパワーを＋5000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"during your turn, this card gets +{power} power"
            }
        ];
    }
}
