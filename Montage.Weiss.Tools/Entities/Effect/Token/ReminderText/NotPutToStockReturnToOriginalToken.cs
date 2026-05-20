namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches "if you do not put 1 card to your stock, return the revealed card to its original place" reminder clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>ストック置場に置かないなら元に戻す</c></para>
/// <para><b>Regex:</b> ^ストック置場に置かないなら元に戻す</para>
/// <para><b>Output:</b> <c>If you do not put 1 card to your stock, return the revealed card to its original place</c></para>
/// </remarks>
internal class NotPutToStockReturnToOriginalToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^ストック置場に置かないなら元に戻す");
    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) =>
        "If you do not put 1 card to your stock, return the revealed card to its original place";
}
