namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST 1 of your other [STAND] trait character" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの【スタンド】している《NIKKE》のキャラを 1 枚【レスト】する</c></para>
/// <para><b>Regex:</b> ^他のあなたの【スタンド】している《(.+?)》のキャラを 1 枚【レスト】する (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>[REST] 1 of your other [STAND] &lt;&lt;trait&gt;&gt; characters</c></para>
/// </remarks>
internal class CostRestStandNikkeCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの【スタンド】している《(.+?)》のキャラを1枚【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの【スタンド】している《★TESTTRAIT★》のキャラを1枚【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 1 of your other [STAND] <<{trait}>> characters"
            }
        ];
    }
}
