namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches the Inheritance (継承) marker transfer clause: choose markers under this card
/// and this card itself, then place them under another character as markers, with cleanup.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの下のマーカー1枚までとこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置き、あなたは自分の山札の上から1枚を、ストック置場に置く。</c></para>
/// <para><b>Regex:</b> ^このカードの下のマーカー(?&lt;count&gt;\d+)枚(?:まで)?とこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置(?:き、あなたは自分の山札の上から(?:\d+)枚を、ストック置場に置|く)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Marker count to transfer (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may choose [up to] N marker under this card and this card, and put them face-up under that character as markers in any order. After that, put the remaining markers to your waiting room[ and put the top card of your deck to your stock].</c></para>
/// </remarks>
internal class ChooseMarkerAndThisCardAndPlaceAsMarkerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの下のマーカー(?<count>\d+)枚(?:まで)?とこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置(?:き、あなたは自分の山札の上から(?:\d+)枚を、ストック置場に置く|く)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches =>
    [
        "このカードの下のマーカー1枚までとこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置く。",
        "このカードの下のマーカー1枚までとこのカードを選び、その舞台に置かれたキャラの下にマーカーとして好きな順番で表向きに置いてよい。そうしたら、残りのマーカーを控え室に置き、あなたは自分の山札の上から1枚を、ストック置場に置く。"
    ];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var text = span.ToString();
        var isUpTo = text.Contains("まで");
        var hasStock = text.Contains("ストック置場に置く");
        var countText = isUpTo ? $"up to {count}" : count;
        var stockText = hasStock ? ", and put the top card of your deck to your stock" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {countText} marker under this card and this card, and put them face-up under that character as markers in any order. After that, put the remaining markers to your waiting room{stockText}"
            }
        ];
    }
}
