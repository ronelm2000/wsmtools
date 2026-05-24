namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "perform the following action" clauses with optional repeat count.
/// Translates inner quoted sub-abilities via <see cref="PowerBoostWithFollowingAbilityToken.TryTranslateNested"/>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の行動を2回行う。『あなたは自分の山札の上から1枚を公開する。…』</c></para>
/// <para><b>Regex:</b> ^次の行動を(?:(?&lt;count&gt;\d+)回)?行う。『(?&lt;inner&gt;.+?)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Optional repeat count (e.g., "2")</description></item>
///   <item><description>inner: Inner quoted action content</description></item>
/// </list>
/// <para><b>Output:</b> <c>perform the following action 2 times. "Reveal the top card of your deck. If that card is a &lt;&lt;NIKKE&gt;&gt; character, put it to your hand."</c></para>
/// </remarks>
internal class PerformFollowingActionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の行動を(?:(?<count>\d+)回)?行う。『(?<inner>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["次の行動を行う。『あなたは自分の控え室の《★TESTTRAIT★》のキャラを1枚選び、手札に戻す。』"];
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var inner = match.Groups["inner"].Value;
        var count = match.Groups["count"] is Group cg && cg.Success ? cg.Value : null;
        var innerEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, inner);
        var actionText = $"perform the following action. \"{innerEffect.EffectText}\"";
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = count != null
                    ? $"perform the following action {count} times. \"{innerEffect.EffectText}\""
                    : actionText,
                NestedEffect = innerEffect,
                IsUnmatched = innerEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
