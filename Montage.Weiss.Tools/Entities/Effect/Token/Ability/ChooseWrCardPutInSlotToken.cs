namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a named card from your waiting room and put it into this card's previous slot position" clauses.
/// Used for effects that retrieve a specific named character from waiting room and place it where this card was.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の控え室の「カード名」を1枚選び、このカードがいた枠に置く。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚選び、このカードがいた枠に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., "カード名")</description></item>
///   <item><description>Group 2: Card count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 "カード名" in your waiting room, and put it into this card's previous position</c></para>
/// </remarks>
internal class ChooseWrCardPutInSlotToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚選び、このカードがいた枠に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の控え室の「カード名」を1枚選び、このカードがいた枠に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} \"{name}\" in your waiting room, and put it into this card's previous position"
            }
        ];
    }
}
