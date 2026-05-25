namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "soul does not decrease by side attack" clauses, with optional duration prefix.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのターン中、このカードはサイドアタックしてもソウルが減少しない。</c></para>
/// <para><b>Regex:</b> ^(そのターン中、)?(?:このカードは)?サイドアタックしてもソウルが減少しない(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Optional duration prefix ("そのターン中、")</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card's soul does not decrease by side attacking</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different subject references
/// - Different phrasing for "soul reduction"</para>
/// </remarks>
internal class CannotReduceSoulBySideAttackToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(そのターン中、)?(?:このカードは)?サイドアタックしてもソウルが減少しない(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードはサイドアタックしてもソウルが減少しない。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var hasDuration = match.Groups[1].Success;
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasDuration
                    ? "this card's soul does not decrease by side attacking until end of turn"
                    : "this card's soul does not decrease by side attacking"
            }
        ];
    }
}
