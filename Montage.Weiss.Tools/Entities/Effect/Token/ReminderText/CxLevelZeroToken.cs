namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class CxLevelZeroToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^CXのレベルは0として扱う");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) => "CX are regarded as level 0";
}
