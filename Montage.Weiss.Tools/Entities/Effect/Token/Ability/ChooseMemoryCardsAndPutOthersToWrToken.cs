namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose N cards in memory, put all other memory cards to waiting room" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の思い出置場のカードを4枚選び、それらのカード以外の自分の思い出置場のカードすべてを、控え室に置く</c></para>
/// <para><b>Regex:</b> ^自分の思い出置場のカードを(\d+)枚選び、それらのカード以外の自分の思い出置場のカードすべてを、控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of cards to keep in memory (e.g., "4")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose N cards in your memory, and put all cards in your memory except those cards to your waiting room</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different source zones (memory vs clock vs stock)
/// - Different exclusion phrasing (それらのカード以外の vs そのカード以外の)
/// - Different destination zones (控え室 vs 山札)</para>
/// </remarks>
internal class ChooseMemoryCardsAndPutOthersToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の思い出置場のカードを(\d+)枚選び、それらのカード以外の自分の思い出置場のカードすべてを、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} card{(count > 1 ? "s" : "")} in your memory, and put all cards in your memory except those cards to your waiting room"
            }
        ];
    }
}
