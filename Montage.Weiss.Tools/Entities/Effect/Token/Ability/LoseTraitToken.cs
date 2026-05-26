namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches clauses where this card loses all instances of a given trait.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは《服》をすべて失い</c></para>
/// <para><b>Regex:</b> ^このカードは《(.+?)》をすべて失い(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "服")</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card loses &lt;&lt;{trait}&gt;&gt;</c></para>
/// </remarks>
internal class LoseTraitToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは《(.+?)》をすべて失い(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["このカードは《服》をすべて失い"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card loses <<{trait}>>"
            }
        ];
    }
}
