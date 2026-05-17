namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches damage cancellation reminder text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>ダメージキャンセルは発生する</c></para>
/// <para><b>Regex:</c> ^ダメージキャンセルは発生する</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>Damage may be canceled</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different phrasing (ダメージがキャンセルされる場合がある, キャンセル発生)</para>
/// </remarks>
internal class DamageMayBeCanceledToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^ダメージキャンセルは発生する");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) => "Damage may be canceled";
}
