namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "this card gets the following N abilities" clauses with two quoted sub-abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは次の2つの能力を得る。『【永】 あなたのターン中、このカードのパワーを＋5000。』『【自】 アンコール ［手札のキャラを1枚控え室に置く］』</c></para>
/// <para><b>Regex:</b> ^このカードは次の(?&lt;count&gt;\d+)つの能力を得る。『(?&lt;e1&gt;.+?)』『(?&lt;e2&gt;.+?)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Number of abilities (e.g., "2")</description></item>
///   <item><description>e1: First quoted ability text</description></item>
///   <item><description>e2: Second quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets the following abilities. "[ability1]" "[ability2]"</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - More than 2 abilities</para>
/// </remarks>
internal class GainStandaloneFollowingAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは次の(?<count>\d+)つの能力を得る。『(?<e1>.+?)』『(?<e2>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードは次の2つの能力を得る。『【永】 あなたのターン中、このカードのパワーを＋5000。』『【自】 アンコール ［手札の《★TESTTRAIT★》のキャラを1枚控え室に置く］』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var e1 = match.Groups["e1"].Value;
        var e2 = match.Groups["e2"].Value;
        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e2);

        var nestedEnglish1 = nestedEffect1.EffectText;
        if (!nestedEnglish1.EndsWith('.') && !nestedEnglish1.EndsWith('"') && !nestedEnglish1.EndsWith(']'))
            nestedEnglish1 += ".";

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following abilities. \"{nestedEnglish1}\" \"{nestedEffect2.EffectText}\"",
                NestedEffect = nestedEffect1,
                IsUnmatched = nestedEffect1.Abilities.Any(a => a.IsUnmatched) || nestedEffect2.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
