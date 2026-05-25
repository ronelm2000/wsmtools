namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches the Inheritance (継承) marker transfer clause: choose markers under this card
/// and this card itself, then place them under another character as markers, with cleanup.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの下のマーカー1枚までとこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置く。</c></para>
/// <para><b>Regex:</b> ^このカードの下のマーカー(?&lt;count&gt;\d+)枚(?:まで)?とこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Marker count to transfer (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may choose [up to] N marker under this card and this card, and put them face-up under that character as markers in any order. After that, put the remaining markers to your waiting room.</c></para>
/// </remarks>
internal class ChooseMarkerAndThisCardAndPlaceAsMarkerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー(?<count>\d+)枚(?:まで)?とこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置く(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["このカードの下のマーカー1枚までとこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var isUpTo = span.ToString().Contains("まで");
        var countText = isUpTo ? $"up to {count}" : count;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {countText} marker under this card and this card, and put them face-up under that character as markers in any order. After that, put the remaining markers to your waiting room"
            }
        ];
    }
}
