namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches "cannot declare negative numbers" reminder text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>マイナスの数は宣言できない</c></para>
/// <para><b>Regex:</c> ^マイナスの数は宣言できない</para>
/// <para><b>Output:</b> <c>You cannot declare a negative number</c></para>
/// </remarks>
internal class CannotDeclareNegativeNumberToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^マイナスの数は宣言できない");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) => "You cannot declare a negative number";
}
