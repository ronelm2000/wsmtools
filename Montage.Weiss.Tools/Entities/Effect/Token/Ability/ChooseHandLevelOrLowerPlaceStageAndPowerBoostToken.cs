namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character from hand with level ≤ your level, put on any stage position, power boost" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の手札の自分のレベル以下のレベルのキャラを1枚まで選び、舞台の好きな枠に置き、そのターン中、このカードのパワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^自分の手札の自分のレベル以下のレベルのキャラを(\d+)枚まで選び、舞台の好きな枠に置き、そのターン中、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character count (e.g., <c>1</c>)</description></item>
///   <item><description>Group 2: Power boost value (e.g., <c>2000</c>)</description></item>
/// </list>
/// <para><b>Output:</b> Three atomic abilities: choose, place on stage, and self power boost.</para>
/// </remarks>
internal class ChooseHandLevelOrLowerPlaceStageAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の手札の自分のレベル以下のレベルのキャラを(\d+)枚まで選び、舞台の好きな枠に置き、そのターン中、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分の手札の自分のレベル以下のレベルのキャラを1枚まで選び、舞台の好きな枠に置き、そのターン中、このカードのパワーを＋2000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} character from your hand with level equal to or less than your level"
            },
            new CardEffectAbility
            {
                AbilityText = "put it on any position on your stage"
            },
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power until end of turn"
            }
        ];
    }
}
