namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a named card from your hand and put it into this card's previous slot position" clauses.
/// Supports optional "up to" count for partial selection from hand.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の手札の「カード名」を1枚まで選び、このカードがいた枠に置く。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?手札の「(.+?)」を(\d+)枚(?:まで)?選び、このカードがいた枠に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., "カード名")</description></item>
///   <item><description>Group 2: Card count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose up to 1 "カード名" in your hand, and put it into this card's previous position</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Without "まで" (exact count instead of up to)
/// - Variable X counts</para>
/// </remarks>
internal class ChooseFromHandToSlotToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?手札の「(.+?)」を(\d+)枚(?:まで)?選び、このカードがいた枠に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の手札の「カード名」を1枚まで選び、このカードがいた枠に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} \"{name}\" in your hand, and put it into this card's previous position"
            }
        ];
    }
}
