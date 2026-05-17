namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class ReturnToOriginalPositionToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^公開したカードは元に戻す");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) => "Return the revealed card to its original place";
}
