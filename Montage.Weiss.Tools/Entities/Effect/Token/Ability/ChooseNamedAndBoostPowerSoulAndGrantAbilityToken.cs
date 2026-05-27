namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a named character, boost power and soul, and grant a quoted follow-up ability" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>「魂魄妖夢」を1枚選び、そのターン中、パワーを＋3000し、ソウルを＋1し、次の能力を与える。『【自】［(1)］ このカードがアタックした時、あなたはコストを払ってよい。そうしたら、そのアタック中、あなたはトリガーステップにトリガーチェックを2回行う。』</c></para>
/// <para><b>Regex:</b> ^「(.+?)」を(\d+)枚選び、そのターン中、パワーを＋(\d+)し、ソウルを＋(\d+)し、次の能力を与える。『(.+?)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Character name in 「」 (e.g., <c>魂魄妖夢</c>)</description></item>
///   <item><description>Group 2: Character count (e.g., <c>1</c>)</description></item>
///   <item><description>Group 3: Power boost value (e.g., <c>3000</c>)</description></item>
///   <item><description>Group 4: Soul boost value (e.g., <c>1</c>)</description></item>
///   <item><description>Group 5: Nested quoted ability text (e.g., <c>【自】［(1)］ このカードがアタックした時...</c>)</description></item>
/// </list>
/// <para><b>Output:</b> A single <see cref="NestedCardEffectAbility"/> with the full English text and sub-translated nested effect.</para>
/// <para><b>Sub-translation:</b> The quoted ability is translated recursively via <see cref="PowerBoostWithFollowingAbilityToken.TranslateNested"/>.</para>
/// </remarks>
internal class ChooseNamedAndBoostPowerSoulAndGrantAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^「(.+?)」を(\d+)枚選び、そのターン中、パワーを＋(\d+)し、ソウルを＋(\d+)し、次の能力を与える。『(.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["「魂魄妖夢」を1枚選び、そのターン中、パワーを＋3000し、ソウルを＋1し、次の能力を与える。『【自】［(1)］ このカードがアタックした時、あなたはコストを払ってよい。そうしたら、そのアタック中、あなたはトリガーステップにトリガーチェックを2回行う。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        var power = match.Groups[3].Value;
        var soul = match.Groups[4].Value;
        var nestedJapanese = match.Groups[5].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {count} of your \"{name}\", that character gets +{power} power, +{soul} soul, and the following ability until end of turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
