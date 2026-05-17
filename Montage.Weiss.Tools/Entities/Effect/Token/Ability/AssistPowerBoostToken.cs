namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches assist power boost clauses with variable X or fixed number.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×500 に等しい。</c> or <c>このカードの前のあなたのキャラすべてに、パワーを＋500。</c></para>
/// <para><b>Regex:</b> ^このカードの前のあなたのキャラすべてに、パワーを＋(X|Ｘ|\d+)(?:。(X|Ｘ) はそのキャラのレベル×(\d+) に等しい)?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power value - X, full-width X, or fixed number</description></item>
///   <item><description>Group 2: X variable marker (if present)</description></item>
///   <item><description>Group 3: Level multiplier (if present)</description></item>
/// </list>
/// <para><b>Output:</b> <c>All of your characters in front of this card get +X power. X is equal to that character's level x500</c> or <c>All of your characters in front of this card get +500 power</c></para>
/// </remarks>
internal class AssistPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの前のあなたの(?:《(.+?)》の)?キャラすべてに、パワーを＋(X|Ｘ|\d+)(?:。(X|Ｘ)はそのキャラのレベル×(\d+)に等しい)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var powerValue = match.Groups[2].Value;
        var hasLevelMultiplier = match.Groups[3].Success && match.Groups[4].Success;
        
        var isFixedNumber = !powerValue.Equals("X", StringComparison.Ordinal) && 
                            !powerValue.Equals("Ｘ", StringComparison.Ordinal) &&
                            int.TryParse(powerValue, out _);
        
        var subject = string.IsNullOrEmpty(trait)
            ? "All of your characters in front of this card"
            : $"All of your <<{trait}>> characters in front of this card";
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = isFixedNumber
                    ? $"{subject} get +{powerValue} power"
                    : hasLevelMultiplier
                        ? $"{subject} get +X power. X is equal to that character's level ×{match.Groups[4].Value}"
                        : $"{subject} get +X power"
            }
        ];
    }
}
