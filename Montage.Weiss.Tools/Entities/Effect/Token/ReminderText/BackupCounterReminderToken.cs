namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches backup/counter reminder text for characters that are being frontal attacked.
/// Now supports variable power (Ｘ) and count (Ｘ) for flexible backup values.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分のフロントアタックされているキャラを1枚選び、そのターン中、パワーを＋Ｘ</c></para>
/// <para><b>Regex:</b> ^あなたは自分のフロントアタックされているキャラを([Ｘ\d]+)枚選び、そのターン中、パワーを＋([Ｘ\d]+)</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character count to choose (numeric or "Ｘ")</description></item>
///   <item><description>Group 2: Power boost value (numeric or "Ｘ")</description></item>
/// </list>
/// <para><b>Output (numeric):</b> <c>Choose 1 of your characters that is being frontal attacked, and that character gets +1000 power until end of turn</c></para>
/// <para><b>Output (variable X):</b> <c>Choose 1 of your characters that is being frontal attacked, and that character gets +X power until end of turn</c></para>
/// </remarks>
internal class BackupCounterReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^あなたは自分のフロントアタックされているキャラを([Ｘ\d]+)枚選び、そのターン中、パワーを＋([Ｘ\d]+)");
    public override IEnumerable<string> SampleMatches => ["あなたは自分のフロントアタックされているキャラを1枚選び、そのターン中、パワーを＋1000", "あなたは自分のフロントアタックされているキャラを1枚選び、そのターン中、パワーを＋Ｘ"];

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value.Replace("Ｘ", "X");
        var power = match.Groups[2].Value.Replace("Ｘ", "X");
        var countNum = int.TryParse(count, out var cn) ? cn : 0;
        var powerNum = int.TryParse(power, out var pn) ? pn : 0;
        if (countNum > 0 && powerNum > 0)
            return $"Choose {countNum} of your characters that is being frontal attacked, and that character gets +{powerNum} power until end of turn";
        return $"Choose {count} of your characters that is being frontal attacked, and that character gets +{power} power until end of turn";
    }
}
