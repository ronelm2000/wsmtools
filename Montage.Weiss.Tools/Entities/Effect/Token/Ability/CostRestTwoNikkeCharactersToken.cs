namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST N trait characters" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの《NIKKE》のキャラを 2 枚【レスト】する</c></para>
/// <para><b>Regex:</b> ^あなたの《(.+?)》のキャラを 2 枚【レスト】する (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>[REST] 2 of your &lt;&lt;trait&gt;&gt; characters</c></para>
/// </remarks>
internal class CostRestTwoNikkeCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの《(.+?)》のキャラを2枚【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたの《★TESTTRAIT★》のキャラを2枚【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 2 of your <<{trait}>> characters"
            }
        ];
    }
}
