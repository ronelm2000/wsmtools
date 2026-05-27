namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches the second sentence of the bond mechanic reminder text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そうしたら、あなたは自分の控え室の「魂魄妖夢」を1枚選び、手札に戻す</c></para>
/// <para><b>Regex:</b> ^そうしたら、あなたは自分の控え室の「(.+?)」を(\d+)枚選び、手札に戻す</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card name in 「」 (e.g., <c>魂魄妖夢</c>)</description></item>
///   <item><description>Group 2: Card count (e.g., <c>1</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>If you do, choose N "name" in your waiting room, and return it to your hand</c></para>
/// <para><b>Name handling:</b> The captured card name is passed through <c>MatchNameFragment</c> for consistency,
/// though bond names are typically not translated.</para>
/// </remarks>
internal class BondReminderPart2Token : CardTextToken<string>
{
    public override Regex Matcher => new(@"^そうしたら、あなたは自分の控え室の「(.+?)」を(\d+)枚選び、手札に戻す");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return $"If you do, choose {match.Groups[2].Value} \"{match.Groups[1].Value}\" in your waiting room, and return it to your hand";
    }
}
