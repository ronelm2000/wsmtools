namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose other trait character, boost power until end of opponent's next turn, and grant nested ability" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは他の自分の《サマポケ》のキャラを1枚選び、次の相手のターンの終わりまで、パワーを＋2500し、次の能力を与える。『【自】［(1)］ アンコールステップの始めに、他のあなたの前列の【レスト】しているキャラがいないなら、あなたはコストを払ってよい。そうしたら、このカードを【レスト】する。』</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?他の自分の《(.+?)》のキャラを(\d+)枚選び、次の相手のターンの終わりまで、パワーを[＋\+](\d+)し、次の能力を与える。『(.+?)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name</description></item>
///   <item><description>Group 2: Character count</description></item>
///   <item><description>Group 3: Power boost value</description></item>
///   <item><description>Group 4: Nested Japanese ability text inside 『』</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose {count} of your other &lt;&lt;{trait}&gt;&gt; characters, and that character gets +{power} power until end of your opponent's next turn, and gets the following ability. "{nestedEffect.EffectText}"</c></para>
/// <para><b>Note:</b> Uses <c>PowerBoostWithFollowingAbilityToken.TranslateNested</c> to parse the nested ability inside 『』.</para>
/// </remarks>
internal class ChooseOtherTraitCharPowerAndGrantAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?他の自分の《(.+?)》のキャラを(\d+)枚選び、次の相手のターンの終わりまで、パワーを[＋\+](\d+)し、次の能力を与える。『(.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = int.Parse(match.Groups[2].Value);
        var power = match.Groups[3].Value;
        var nestedJapanese = match.Groups[4].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {count} of your other <<{trait}>> characters, and that character gets +{power} power until end of your opponent's next turn, and gets the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
