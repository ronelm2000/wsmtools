namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "may reveal up to N cards, if N+ revealed then conditional ability" two-sentence chains.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札の上から3枚までを、公開してよい。1枚以上公開したなら、あなたはそれらのカードの《幻想郷》のキャラを1枚まで選び、手札に加え、残りのカードを控え室に置き、自分の手札を1枚選び、控え室に置く。</c></para>
/// <para><b>Regex:</b> ^自分の山札の上から(\d+)枚までを、公開してよい。(\d+)枚以上公開したなら、(.+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Reveal count (e.g., <c>3</c>)</description></item>
///   <item><description>Group 2: Threshold count (e.g., <c>1</c>)</description></item>
///   <item><description>Group 3: Conditional ability text (sub-translated via <see cref="EffectListRegistry"/>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may reveal up to N cards</c>, <c>If you do</c> (prefix), then the sub-translated abilities.</para>
/// <para><b>Note:</b> The <c>。</c> in the pattern is protected by <see cref="MultiClauseEffectParser"/> when it appears
/// inside certain known patterns like <c>コストを払ってよい。</c>, but not for generic <c>公開してよい。</c>.
/// This token exists as a backup; consider using <see cref="RevealUpToNAndMayToken"/> + <see cref="RevealedCountConditionToken"/>
/// for sentence-split parsing.</para>
/// </remarks>
internal class MayRevealUpToThenConditionalChainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の山札の上から(\d+)枚までを、公開してよい。(\d+)枚以上公開したなら、(.+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分の山札の上から3枚までを、公開してよい。1枚以上公開したなら、あなたはそれらのカードの《幻想郷》のキャラを1枚まで選び、手札に加え、残りのカードを控え室に置き、自分の手札を1枚選び、控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var upTo = match.Groups[1].Value;
        var threshold = match.Groups[2].Value;
        var conditionalAbilityJp = match.Groups[3].Value;

        var abilities = new List<CardEffectAbility>
        {
            new CardEffectAbility
            {
                AbilityText = $"you may reveal up to {upTo} cards from the top of your deck"
            }
        };

        abilities.Add(new CardEffectAbility
        {
            Prefix = AbilityPrefix.IfYouDo,
            AbilityText = string.Empty
        });

        var subAbilities = SubTranslateAbility(registry, conditionalAbilityJp);
        abilities.AddRange(subAbilities);

        return abilities;
    }

    private static List<CardEffectAbility> SubTranslateAbility(ITokenRegistry registry, string abilityJp)
    {
        var abilities = new List<CardEffectAbility>();
        var remaining = abilityJp;
        var maxIterations = 20;
        var iteration = 0;

        while (!string.IsNullOrWhiteSpace(remaining) && iteration < maxIterations)
        {
            iteration++;
            var trimmed = remaining.TrimStart();
            if (trimmed.Length == 0) break;

            var matchResult = registry.EffectListRegistry.Match(trimmed.AsMemory());
            if (matchResult != null)
            {
                var abilList = matchResult.Translate(registry);
                abilities.AddRange(abilList);
                remaining = trimmed[matchResult.Match.Length..].TrimStart('、', '。', ' ', '\t');
            }
            else
            {
                break;
            }
        }

        return abilities;
    }
}
