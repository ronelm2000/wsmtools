namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class CxLevelZeroToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^CX のレベルは 0 として扱う");

    public override string Translate(ITokenRegistry registry, Match match) => "CX are regarded as level 0";
}
