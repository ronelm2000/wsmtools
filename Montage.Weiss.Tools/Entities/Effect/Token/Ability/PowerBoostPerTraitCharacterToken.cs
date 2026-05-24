namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Per other character with a specific trait" power boost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの《風》のキャラ1枚につき、このカードのパワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^他のあなたの《(.+?)》のキャラ 1 枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "風")</description></item>
///   <item><description>Group 2: Power value (e.g., "2000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>This card gets +2000 power for each of your other <<風>> characters</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different character counts (e.g., "2 枚につき")
/// - Different subject references (e.g., "自分の" instead of "他のあなたの")
/// - Different trait phrasings (e.g., "《風》のキャラ" vs "《風》character")</para>
/// </remarks>
internal class PowerBoostPerTraitCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラ1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの《★TESTTRAIT★》のキャラ1枚につき、このカードのパワーを＋2000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = registry.MatchNameFragment(match.Groups[1].Value);
            var power = match.Groups[2].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"This card gets +{power} power for each of your other <<{trait}>> characters"
                }
            ];
        }
        return [];
    }
}
