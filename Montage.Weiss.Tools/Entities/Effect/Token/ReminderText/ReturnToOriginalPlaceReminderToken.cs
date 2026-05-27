namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class ReturnToOriginalPlaceReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^見たカードは元に戻す\)?");
    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return "Return them to their original places";
    }
}
