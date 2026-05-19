namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "all players perform the following action" clauses with nested sub-action.
/// The nested sub-action text in 『...』 is translated via <see cref="PowerBoostWithFollowingAbilityToken.TryTranslateNested"/>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>すべてのプレイヤーは次の行動を行う。『あなたの思い出が5枚以上なら、...』</c></para>
/// <para><b>Regex:</b> ^すべてのプレイヤーは次の行動を行う。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted action text</description></item>
/// </list>
/// <para><b>Output:</b> <c>All players perform the following action. "..."</c></para>
/// </remarks>
internal class AllPlayersPerformActionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^すべてのプレイヤーは次の行動を行う。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Success ? match.Groups["nested"].Value : null;
        var baseText = "All players perform the following action";
        if (nestedJapanese != null)
        {
            var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{baseText}. \"{nestedEnglish}\""
                }
            ];
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = baseText
            }
        ];
    }
}
