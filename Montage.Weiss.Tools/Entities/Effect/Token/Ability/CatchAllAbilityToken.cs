using System.Text;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Catch-all token that matches any remaining ability text that no specific ability token recognized.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> Any text that reached the ability loop's break point after all
/// direct-match and prefix-skip attempts failed.</para>
/// <para><b>Regex:</c> ^.+</para>
/// <para><b>Captures:</b> None (the entire match is the unrecognized text).</para>
/// <para><b>Output:</b> A <see cref="UnmatchedAbility"/> with <see cref="CardEffectAbility.IsUnmatched"/> set to <c>true</c>
/// and <see cref="UnmatchedAbility.Suggestions"/> containing diagnostic hints.</para>
/// <para><b>Registration:</b> NOT registered in <see cref="IComponentRegistry{T}"/> — doing so would
/// cause false positives by matching before the prefix-skip pipeline runs in <see cref="MultiClauseEffectParser.ParseSentence"/>.
/// Instead, invoked explicitly at the ability loop's break point in <see cref="MultiClauseEffectParser"/>.</para>
/// <para><b>Purpose:</b> Development aid — logs a warning and produces a sentinel ability that causes a
/// <see cref="NotImplementedException"/> after the full <see cref="CardEffectTree"/> is built,
/// showing developers which ability text needs a new token.</para>
/// </remarks>
public class CatchAllAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CatchAllAbilityToken>();

    public override Regex Matcher => new(@"^.+");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var text = span.ToString();
        var suggestionItems = GenerateSuggestionItems(text);
        foreach (var s in suggestionItems)
            Log.Warning("CatchAllAbilityToken: suggestion: {Suggestion}", s);
        return
        [
            new UnmatchedAbility
            {
                AbilityText = text,
                IsUnmatched = true,
                Suggestions = [.. suggestionItems]
            }
        ];
    }

    /// <summary>
    /// Generates a detailed human-readable suggestion string for log output.
    /// </summary>
    public static string GenerateSuggestions(string text)
    {
        var sb = new StringBuilder();
        DetectConditionalAbilityPattern(text, sb);
        DetectMultiClausePattern(text, sb);
        DetectPrefixHints(text, sb);
        DetectConditionLikeEnding(text, sb);
        DetectDurationPatterns(text, sb);
        return sb.ToString();
    }

    /// <summary>
    /// Generates sparse, terse suggestion items suitable for embedding in
    /// an <see cref="UnmatchedAbility"/> record for JSON serialization.
    /// Each item is a short diagnostic hint string.
    /// </summary>
    public static IEnumerable<string> GenerateSuggestionItems(string text)
    {
        if (DetectConditionalAbilityPattern(text, out var condClause, out var abilClause))
        {
            yield return $"ConditionalAbilityText: {condClause}, {abilClause}";
        }

        var clauseCount = CountClauses(text);
        if (clauseCount > 1)
        {
            yield return $"Multi-clause: {clauseCount} clauses separated by 、or 。";
        }

        var trimmed = text.TrimEnd('、', '。', ' ', '\t');
        if (trimmed.EndsWith("なら") || trimmed.EndsWith("場合"))
            yield return "ConditionEnding (Type.If)";
        else if (trimmed.EndsWith("時") || trimmed.EndsWith("たび"))
            yield return "ConditionEnding (Type.When)";
        else if (trimmed.EndsWith("限り"))
            yield return "ConditionEnding (Type.During)";

        if (text.StartsWith("あなたは"))
            yield return "StartsWith: 'あなたは'";
        else if (text.StartsWith("あなたの") || text.StartsWith("自分の"))
            yield return "StartsWith: subject prefix";
        else if (text.StartsWith("このカード"))
            yield return "StartsWith: 'このカード'";

        if (text.Contains("ターン中") || text.Contains("終わりまで") || text.Contains("バトル中"))
            yield return "ContainsDurationPhrase";
    }

    private static int CountClauses(string text)
    {
        var trimmed = text.TrimEnd('。', '、', ' ', '\t');
        if (trimmed.Length == 0) return 1;
        return trimmed.Split(['、', '。'], StringSplitOptions.RemoveEmptyEntries)
                      .Count(s => s.Trim().Length > 0);
    }

    private static void DetectMultiClausePattern(string text, StringBuilder sb)
    {
        var clauseCount = CountClauses(text);
        if (clauseCount > 1)
            sb.AppendLine($"- Multi-clause: {clauseCount} clauses separated by 、or 。");
    }

    private static bool DetectConditionalAbilityPattern(string text, out string conditionClause, out string abilityClause)
    {
        conditionClause = "";
        abilityClause = "";
        var match = Regex.Match(text, @"^(?<condition>.+?)(?:なら|場合)、(?:、)?(?<ability>.+)$");
        if (!match.Success)
        {
            match = Regex.Match(text, @"^(?<condition>.+?)(?:時|たび|限り)(?:、)?(?<ability>.+)$");
        }
        if (match.Success)
        {
            conditionClause = match.Groups["condition"].Value.Trim();
            abilityClause = match.Groups["ability"].Value.Trim().TrimEnd('。', '、');
            return true;
        }
        return false;
    }

    private static void DetectConditionalAbilityPattern(string text, StringBuilder sb)
    {
        if (DetectConditionalAbilityPattern(text, out var condClause, out var abilClause))
        {
            sb.AppendLine($"- ConditionalAbilityText: {condClause}, {abilClause}");
        }
    }

    private static void DetectPrefixHints(string text, StringBuilder sb)
    {
        if (text.StartsWith("あなたは", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: 'あなたは'");
        else if (text.StartsWith("あなたの", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: 'あなたの' (subject prefix)");
        else if (text.StartsWith("自分の", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: '自分の' (subject prefix)");
        else if (text.StartsWith("このカード", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: 'このカード'");
        else if (text.StartsWith("そうしたら", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: 'そうしたら' (IfYouDo conjunction)");
        else if (text.StartsWith("そうで", StringComparison.Ordinal))
            sb.AppendLine("- Starts with: 'そうで...' (Otherwise conjunction)");
    }

    private static void DetectConditionLikeEnding(string text, StringBuilder sb)
    {
        var trimmed = text.TrimEnd('、', '。', ' ', '\t');
        if (trimmed.EndsWith("なら", StringComparison.Ordinal))
            sb.AppendLine("- Ends with: 'なら' (condition marker, Type.If)");
        else if (trimmed.EndsWith("時", StringComparison.Ordinal))
            sb.AppendLine("- Ends with: '時' (condition marker, Type.When)");
        else if (trimmed.EndsWith("たび", StringComparison.Ordinal))
            sb.AppendLine("- Ends with: 'たび' (condition marker, Type.When)");
        else if (trimmed.EndsWith("限り", StringComparison.Ordinal))
            sb.AppendLine("- Ends with: '限り' (condition marker, Type.During)");
        else if (trimmed.EndsWith("場合", StringComparison.Ordinal))
            sb.AppendLine("- Ends with: '場合' (condition marker, Type.If)");
    }

    private static void DetectDurationPatterns(string text, StringBuilder sb)
    {
        if (text.Contains("ターン中", StringComparison.Ordinal))
            sb.AppendLine("- Contains: 'ターン中' (duration phrase)");
        else if (text.Contains("終わりまで", StringComparison.Ordinal))
            sb.AppendLine("- Contains: '終わりまで' (duration phrase)");
        else if (text.Contains("バトル中", StringComparison.Ordinal))
            sb.AppendLine("- Contains: 'バトル中' (duration phrase)");
    }
}
