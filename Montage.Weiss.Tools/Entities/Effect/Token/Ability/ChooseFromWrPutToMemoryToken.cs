namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a named card from waiting room and put it into memory" clauses.
/// Supports optional subject prefixes (あなたは/自分の) and uses "てよい" (may) suffix.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の控え室の「カード名」を1枚選び、思い出にしてよい。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚選び、思い出にしてよい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., "カード名")</description></item>
///   <item><description>Group 2: Card count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 "カード名" in your waiting room, and put it into your memory</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Without "てよい" (forced action instead of may)
/// - Up to N count selection</para>
/// </remarks>
internal class ChooseFromWrPutToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚選び、思い出にしてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の控え室の「カード名」を1枚選び、思い出にしてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 \"{name}\" in your waiting room, and put it into your memory"
            }
        ];
    }
}
