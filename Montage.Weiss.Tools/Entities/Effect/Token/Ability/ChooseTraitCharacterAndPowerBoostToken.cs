namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a trait character and power boost" clauses with plural-aware subject-verb agreement.
/// Supports an optional "other" qualifier, up-to-N counts, singular/plural pronoun selection,
/// and full-width <c>Ｘ</c> for variable power values. Emits two atomic abilities for proper conjunction handling.
/// Does not require <c>自分の</c> prefix — the prefix system strips it before matching.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>《NIKKE》のキャラを1枚選び、そのターン中、パワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^(?:他の)?《(.+?)》のキャラを(\d+)枚(まで)?選び、そのターン中、パワーを[＋\+]([Ｘ\d]+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
///   <item><description>Group 2: Character count (e.g., "1")</description></item>
///   <item><description>Group 3: Optional "まで" (present for up-to-N)</description></item>
///   <item><description>Group 4: Power boost value (e.g., "2000" or "Ｘ")</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>choose [up to] N of your [other] &lt;&lt;trait&gt;&gt; characters</c></description></item>
///   <item><description><c>that character gets +N power until end of turn</c> (singular) / <c>those characters get +N power until end of turn</c> (plural)</description></item>
/// </list>
/// <para><b>Rationale:</b> Decomposed from a single combined ability into two atomics so the parent token sees the correct count
/// and avoids inserting a spurious extra "and" before "choose".</para>
/// </remarks>
internal class ChooseTraitCharacterAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:他の)?(?:自分の)?《(.+?)》のキャラを(\d+)枚(まで)?選び、そのターン中、パワーを[＋\+]([Ｘ\d]+)(?:。Ｘは他のあなたの《(.+?)》のキャラの枚数×(\d+)に等しい)?(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他の自分の《★TESTTRAIT★》のキャラを1枚選び、そのターン中、パワーを＋2000。", "他の自分の《★TESTTRAIT★》のキャラを1枚選び、そのターン中、パワーを＋Ｘ。Ｘは他のあなたの《★TESTTRAIT★》のキャラの枚数×500に等しい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = int.Parse(match.Groups[2].Value);
        var isUpTo = match.Groups[3].Success;
        var upToText = isUpTo ? "up to " : "";
        var power = match.Groups[4].Value.Replace('Ｘ', 'X');
        var hasOther = match.Value.Contains("他の", StringComparison.Ordinal);
        var otherText = hasOther ? "other " : "";
        var verb = count == 1 ? "that character gets" : "those characters get";
        var hasXDescription = match.Groups[5].Success;
        var xDescription = "";
        if (hasXDescription)
        {
            var xTrait = registry.MatchNameFragment(match.Groups[5].Value);
            var xMultiplier = match.Groups[6].Value;
            xDescription = $". {power} is equal to the number of your other <<{xTrait}>> characters ×{xMultiplier}";
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {upToText}{count} of your {otherText}<<{trait}>> characters",
                Prefix = AbilityPrefix.And
            },
            new CardEffectAbility
            {
                AbilityText = $"{verb} +{power} power until end of turn{xDescription}",
                Prefix = AbilityPrefix.And
            }
        ];
    }
}
