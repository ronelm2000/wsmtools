namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose 1 of N effects" clauses with two quoted sub-effects.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の2つの効果のうちあなたが選んだ1つを行う。『効果A』『効果B』</c></para>
/// <para><b>Regex:</b> ^次の(\d+)つの効果のうちあなたが選んだ1つを行う。『(?&lt;e1&gt;.+?)』『(?&lt;e2&gt;.+?)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Effect count (e.g., "2")</description></item>
///   <item><description>e1: First quoted effect text</description></item>
///   <item><description>e2: Second quoted effect text</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of the following effects, and perform it. "effect1" "effect2"</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - More than 2 effects</para>
/// </remarks>
internal class ChooseOneFromManyEffectsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の(\d+)つの効果のうちあなたが選んだ1つを行う。『(?<e1>.+?)』『(?<e2>.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["次の2つの効果のうちあなたが選んだ1つを行う。『あなたは自分の山札の上から2枚までを、ストック置場に置く。』『あなたは自分の控え室のキャラを1枚まで選び、手札に戻す。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var e1 = match.Groups["e1"].Value;
        var e2 = match.Groups["e2"].Value;
        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e2);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose 1 of the following effects, and perform it. \"{nestedEffect1.EffectText}\" \"{nestedEffect2.EffectText}\"",
                NestedEffect = nestedEffect1,
                IsUnmatched = nestedEffect1.Abilities.Any(a => a.IsUnmatched) || nestedEffect2.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
