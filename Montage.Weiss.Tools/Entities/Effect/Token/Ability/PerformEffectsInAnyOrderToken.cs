namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "perform the following N effects 1 time each in any order" clauses.
/// Used for event effects that list multiple sub-abilities to be performed in any order by the player.
/// The sub-abilities themselves are handled by separate CSV entries or SubAbilityToken for quoted blocks.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の3つの効果を好きな順番で1回ずつ行う。</c></para>
/// <para><b>Regex:</b> ^次の(\d+)つの効果を好きな順番で1回ずつ行う(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of effects (e.g., "3")</description></item>
/// </list>
/// <para><b>Output:</b> <c>perform the following 3 effects 1 time each in any order</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - "行ってよい" (you may) variant
/// - Variable X counts in addition to numeric</para>
/// </remarks>
internal class PerformEffectsInAnyOrderToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の(\d+)つの効果を好きな順番で1回ずつ行う(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["次の3つの効果を好きな順番で1回ずつ行う。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"perform the following {count} effects 1 time each in any order"
            }
        ];
    }
}
