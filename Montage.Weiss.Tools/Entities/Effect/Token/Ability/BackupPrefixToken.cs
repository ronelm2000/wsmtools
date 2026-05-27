namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches backup/counter ability prefix patterns: "backup N, Level M [cost]".
/// Now supports variable backup power (Ｘ) in addition to numeric values.
/// Used within act effect cost brackets and by ActEffectToken's backup handler.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>助太刀2000 レベル2 ［(1) 手札のこのカードを控え室に置く］</c></para>
/// <para><b>Regex:</b> ^助太刀([Ｘ\d]+)\s*レベル(\d+)\s*［(?:\((\d+)\)\s*)?手札のこのカードを控え室に置く］</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Backup power (numeric or "Ｘ")</description></item>
///   <item><description>Group 2: Level requirement</description></item>
///   <item><description>Group 3: Optional stock cost in parentheses</description></item>
/// </list>
/// <para><b>Output:</b> <c>backup X, Level 1 [Put this card in your hand to your waiting room]</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different cost bracket formats</para>
/// </remarks>
internal class BackupPrefixToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^助太刀([Ｘ\d]+)\s*レベル(\d+)\s*［(?:\((\d+)\)\s*)?手札のこのカードを控え室に置く］");
    public override IEnumerable<string> SampleMatches => ["助太刀2000 レベル2 ［(1) 手札のこのカードを控え室に置く］", "助太刀Ｘ レベル1 ［手札のこのカードを控え室に置く］"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var backupPower = match.Groups[1].Value.Replace("Ｘ", "X");
        var level = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"backup {backupPower}, Level {level} [Put this card in your hand to your waiting room]"
            }
        ];
    }
}
