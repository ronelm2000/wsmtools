namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches deck search clauses with trait filtering and reveal actions.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札を上から4枚まで見て、その中から《風》のキャラを1枚まで選んで相手に見せ、手札に加える。</c></para>
/// <para><b>Regex:</c> ^あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Look count (e.g., "4")</description></item>
///   <item><description>Group 2: Full trait phrase (e.g., "《風》のキャラ")</description></item>
///   <item><description>Group 3: Trait name (e.g., "風")</description></item>
///   <item><description>Group 4: Alternative filter phrase</description></item>
///   <item><description>Group 5: Pick count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>search your deck for up to 1 &lt;&lt;風&gt;&gt; character, reveal it to your opponent</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different search phrasing (山札から探す, デッキから選ぶ)
/// - Different reveal actions (相手に示す, 公開する)
/// - Different placement actions (手札に加える, スタックに置く)</para>
/// </remarks>
internal class SearchDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の山札を上から4枚まで見て、その中から《★TESTTRAIT★》のキャラを1枚まで選んで相手に見せ、手札に加える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[3].Success ? registry.MatchNameFragment(match.Groups[3].Value) : registry.MatchNameFragment(match.Groups[4].Value);
        var pickCount = match.Groups[5].Value.Replace("Ｘ", "X");
        
        var fullMatch = match.Groups[0].Value;
        var revealIndex = fullMatch.IndexOf("相手に見せ", StringComparison.Ordinal);
        var additional = string.Empty;
        
        if (revealIndex >= 0)
        {
            var remaining = fullMatch.Substring(revealIndex + 6).Trim();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                // Split by Japanese comma and join with " and "
                var parts = remaining.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    additional = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        additional += " and " + parts[i];
                    }
                }
            }
        }
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {pickCount} <<{trait}>> character from among them, reveal it to your opponent{additional}"
            }
        ];
    }
}

/// <summary>
/// Matches deck search clauses with top-card look and trait filtering.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札を上から4枚まで見て、その中から《風》のキャラを1枚まで選んで相手に見せ</c></para>
/// <para><b>Regex:</c> ^あなたは自分の山札を上から(.+?)枚まで見て、その中から《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Look count (e.g., "4")</description></item>
///   <item><description>Group 2: Trait name (e.g., "風")</description></item>
///   <item><description>Group 3: Pick count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>search your deck for up to 4 cards, choose up to 1 &lt;&lt;風&gt;&gt; character from among them, reveal it to your opponent</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different look phrasing (めくる, 確認する)
/// - Different trait formats (《》without brackets)</para>
/// </remarks>
internal class SearchDeckWithTopLookToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を上から(.+?)枚まで見て、その中から《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたは自分の山札を上から4枚まで見て、その中から《★TESTTRAIT★》のキャラを1枚まで選んで相手に見せ。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = registry.MatchNameFragment(match.Groups[2].Value);
        var pickCount = match.Groups[3].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} cards, choose up to {pickCount} <<{trait}>> character from among them, reveal it to your opponent"
            }
        ];
    }
}
