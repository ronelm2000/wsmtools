namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose N cards in hand and put them to waiting room" clauses, with optional <c>よい</c> (may) suffix.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の手札を1枚選び、控え室に置いてよい</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?自分の手札を(\d+)枚選び、控え室に置(?:いて|き|く)(?:よい)?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of cards (e.g., "1")</description></item>
/// </list>
/// <para><b>Output (may):</b> <c>you may choose 1 card in your hand and put it to your waiting room</c> (single atomic ability)</para>
/// <para><b>Output (forced):</b> <c>choose 1 card in your hand</c> + <c>put it to your waiting room</c> (two atomic abilities)</para>
/// <para><b>Rationale:</b> When <c>よい</c> (may) is present, the two actions merge into one ability to avoid duplicating "you may".
/// When <c>よい</c> is absent, the actions are emitted as separate atomic abilities for proper conjunction handling by the parent token.</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different verb forms (置く, 置いて, 置き)
/// - Different source zones (手札 vs 控え室)</para>
/// </remarks>
internal class ChooseCardAndPutInWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の手札を(\d+)枚選び、控え室に置(?:いて|き|く)(?:よい)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var isMay = match.Value.Contains("よい");
        var mayText = isMay ? "you may " : "";
        if (isMay)
        {
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {count} card{(count > 1 ? "s" : "")} in your hand and put {(count == 1 ? "it" : "them")} to your waiting room"
                }
            ];
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} card{(count > 1 ? "s" : "")} in your hand"
            },
            new CardEffectAbility
            {
                AbilityText = $"put {(count == 1 ? "it" : "them")} to your waiting room"
            }
        ];
    }
}
