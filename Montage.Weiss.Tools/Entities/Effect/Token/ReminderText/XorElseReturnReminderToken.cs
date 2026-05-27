namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class XorElseReturnReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^そうしないなら元に戻す\)?");
    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return "otherwise, return it to its original place";
    }
}
