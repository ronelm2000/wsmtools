namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches multi-action clauses that move THIS card to an open center-stage position,
/// put all opponent characters to WR, and grant a trait.
/// Emits atomic abilities — each returned <see cref="CardEffectAbility"/> represents a single action.
/// The parent (e.g. <see cref="AutoEffectToken"/> via <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/>)
/// joins them with appropriate connectors (", " then ", and ").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたはこのカードを前列のキャラのいない枠に動かし、相手のキャラすべてを、控え室に置き、そのターン中、このカードは《ヒーロー》を得る。</c></para>
/// <para><b>Regex:</b> ^あなたはこのカードを前列のキャラのいない枠に動かし、相手のキャラすべてを、控え室に置き、そのターン中、このカードは《(.+?)》を得る(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait gained (e.g., "ヒーロー")</description></item>
/// </list>
/// <para><b>Output (3 atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description>0: <c>move this card to an open position of your center stage</c></description></item>
///   <item><description>1: <c>put all of your opponent's characters to their waiting room</c></description></item>
///   <item><description>2: <c>this card gets &lt;&lt;ヒーロー&gt;&gt; until end of turn</c></description></item>
/// </list>
/// <para><b>Atomic Ability Pattern:</b> See <c>SearchDeckLevelAndCostToken</c> for the rationale;
/// this token follows the same decomposition pattern.</para>
/// </remarks>
internal class MoveToOpenPositionAndPutOpponentCharactersToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはこのカードを前列のキャラのいない枠に動かし、相手のキャラすべてを、控え室に置き、そのターン中、このカードは《(.+?)》を得る(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["あなたはこのカードを前列のキャラのいない枠に動かし、相手のキャラすべてを、控え室に置き、そのターン中、このカードは《★TESTTRAIT★》を得る。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility { AbilityText = "move this card to an open position of your center stage" },
            new CardEffectAbility { AbilityText = "put all of your opponent's characters to their waiting room" },
            new CardEffectAbility { AbilityText = $"this card gets <<{trait}>> until end of turn" }
        ];
    }
}
