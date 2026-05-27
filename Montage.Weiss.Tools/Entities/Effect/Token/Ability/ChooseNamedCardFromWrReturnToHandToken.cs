namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a named card from waiting room and return it to hand" clauses.
/// Supports the optional "may" suffix (てよい) and te-form continuation (手札に戻し) for multi-clause abilities.
/// Returns two atomic abilities: choose + return, for proper conjunction handling by the parent token.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の控え室の「カード名」を1枚選び、手札に戻す。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚(?:まで)?選び、(?&lt;action&gt;手札に戻してよい|手札に戻す|手札に戻し)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., "カード名")</description></item>
///   <item><description>Group 2: Card count (e.g., "1")</description></item>
///   <item><description>action: Action type — return to hand (plain, te-form, or may variant)</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>(you may) choose 1 "カード名" in your waiting room</c></description></item>
///   <item><description><c>return it to your hand</c></description></item>
/// </list>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Marker placement as alternative action (like ChooseFromWaitingRoomAndReturnToken)
/// - Level or trait filters in addition to named card matching
/// - Variable X counts</para>
/// </remarks>
internal class ChooseNamedCardFromWrReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?控え室の「(.+?)」を(\d+)枚(?:まで)?選び、(?<action>手札に戻してよい|手札に戻す|手札に戻し)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の控え室の「カード名」を1枚選び、手札に戻す。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        var action = match.Groups["action"].Value;
        var mayText = action.EndsWith("てよい") ? "you may " : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"{mayText}choose {count} \"{name}\" in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"return {(count == "1" ? "it" : "them")} to your hand"
            }
        ];
    }
}
