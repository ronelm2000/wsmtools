namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "opponent cannot use [ACT] on stage until end of opponent's next turn" clauses.
/// Output is structured with the duration at the end so it chains naturally as the last item in a serial-comma list.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない。</c></para>
/// <para><b>Regex:</b> ^次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない</para>
/// <para><b>Output:</b> <c>your opponent cannot use [ACT] of characters on their stage until the end of your opponent's next turn</c></para>
/// <para><b>Rationale:</b> Duration trailing avoids unnatural phrasing when joined as a list item:
/// <c>..., and your opponent cannot use [ACT] of characters on their stage until the end of your opponent's next turn.</c>
/// (vs the old duration-leading form: <c>..., and until the end of your opponent's next turn, your opponent cannot use...</c>)</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different duration phrases (そのターン中, 次のターンの終わりまで)
/// - Different restricted actions (【起】 vs 【自】 vs イベント)
/// - Optional leading <c>、</c> when chained after a previous ability</para>
/// </remarks>
internal class CannotUseActUntilEndOfTurnToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "your opponent cannot use [ACT] of characters on their stage until the end of your opponent's next turn"
            }
        ];
    }
}
