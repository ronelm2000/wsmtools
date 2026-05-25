namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "this card gets the following quoted ability" clauses, parsing the inner quoted text as a nested effect.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは『【永】 あなたのターン中、このカードのパワーを＋3000。』を得る。</c></para>
/// <para><b>Regex:</b> ^このカードは『(?&lt;inner&gt;.+?)』を得る(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>inner: The quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets the following ability. "[nested effect text]"</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Plural abilities (複数の能力 instead of singular)</para>
/// </remarks>
internal class GainQuotedAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは『(?<inner>.+?)』を得る(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードは『【永】 あなたのターン中、このカードのパワーを＋3000。』を得る。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var inner = match.Groups["inner"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, inner);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
