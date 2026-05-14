namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class BackupCounterReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^あなたは自分のフロントアタックされているキャラを(\d+) 枚選び、そのターン中、パワーを＋(\d+)");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var power = int.Parse(match.Groups[2].Value);
        return $"Choose {count} of your characters that is being frontal attacked, and that character gets +{power} power until end of turn";
    }
}
