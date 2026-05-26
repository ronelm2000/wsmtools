namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches clauses where this card gains N following abilities (quoted in 『』 blocks).
/// No power boost prefix — purely a "get following abilities" clause.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは次の3つの能力を得る。『...』『...』『...』</c></para>
/// <para><b>Regex:</b> ^このカードは次の(?&lt;count&gt;\d+)つの能力を得る。((?:『[^』]+』)+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Number of abilities (e.g., "3")</description></item>
///   <item><description>Group 1: All concatenated 『...』 blocks</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets the following abilities. "...first..." "...second..." "...third..."</c></para>
/// <para><b>Scope Expansion:</b> Currently handles 1+ abilities. The first ability gets a trailing period added
/// if missing (matching PowerBoostWithFollowingAbilitiesToken convention). Subsequent abilities are quoted as-is.</para>
/// </remarks>
internal class GetFollowingAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは次の(?<count>\d+)つの能力を得る。((?:『[^』]+』)+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches =>
    [
        @"このカードは次の3つの能力を得る。『【永】 あなたのターン中、このカードのパワーを＋2000。』『【自】 このカードがアタックした時、このカードの正面のキャラのレベルが2なら、そのターン中、このカードのパワーを＋6000。』『【自】 ターンの終わりに、このカードを控え室に置く。』"
    ];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var fullText = span.ToString();
        var match = Matcher.Match(fullText);
        var count = int.Parse(match.Groups["count"].Value);
        var blocksText = match.Groups[1].Value;

        var nestedRegex = new Regex(@"『(?<nested>.+?)』");
        var nestedMatches = nestedRegex.Matches(blocksText);
        var nestedEffects = new List<CardEffect>();

        for (int i = 0; i < Math.Min(count, nestedMatches.Count); i++)
        {
            var nestedJapanese = nestedMatches[i].Groups["nested"].Value;
            var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
            nestedEffects.Add(nestedEffect);
        }

        var abilityText = count switch
        {
            1 => "this card gets the following ability",
            _ => "this card gets the following abilities"
        };

        if (nestedEffects.Count > 0)
        {
            var firstEnglish = nestedEffects[0].EffectText;
            if (!firstEnglish.EndsWith('.') && !firstEnglish.EndsWith('"') && !firstEnglish.EndsWith(']') && firstEnglish.Contains(' '))
                firstEnglish += ".";
            abilityText += $". \"{firstEnglish}\"";
            for (int i = 1; i < nestedEffects.Count; i++)
            {
                abilityText += $" \"{nestedEffects[i].EffectText}\"";
            }
        }

        var firstNestedEffect = nestedEffects.Count > 0 ? nestedEffects[0] : null;

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = abilityText,
                NestedEffect = firstNestedEffect,
                IsUnmatched = nestedEffects.Any(ne => ne.Abilities.Any(a => a.IsUnmatched))
            }
        ];
    }
}
