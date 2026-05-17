namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Opponent cannot use ACT abilities on stage until end of turn" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない。</c></para>
/// <para><b>Regex:</b> ^次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Until the end of your opponent's next turn, your opponent cannot use [ACT] abilities of characters on stage</c></para>
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
                AbilityText = "Until the end of your opponent's next turn, your opponent cannot use [ACT] abilities of characters on stage"
            }
        ];
    }
}
