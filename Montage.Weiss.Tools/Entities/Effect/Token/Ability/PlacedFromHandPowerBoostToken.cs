namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "When this card is placed on stage from hand, power boost" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードが手札から舞台に置かれた時、そのターン中、このカードのパワーを＋X。</c></para>
/// <para><b>Regex:</b> ^このカードが手札から舞台に置かれた時、そのターン中、このカードのパワーを＋(\d+|X)(?:。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power value (e.g., "X" or "2000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>When this card is placed on stage from your hand, this card gets +X power until end of turn</c></para>
/// </remarks>
internal class PlacedFromHandPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードが手札から舞台に置かれた時、そのターン中、このカードのパワーを＋(\d+|X)(?:\.|,|、|。)?");

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
                    AbilityText = $"When this card is placed on stage from your hand, this card gets +{power} power until end of turn"
                }
            ];
        }
        return [];
    }
}
