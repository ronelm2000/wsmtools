namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "get +N level until end of turn" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのターン中、レベルを＋1。</c></para>
/// <para><b>Regex:</b> ^(?:そのターン中、)?レベルを[＋\+](?<level>\d+)</para>
/// <para><b>Output:</b> <c>gets +N level until end of turn</c></para>
/// </remarks>
internal class LevelBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:そのターン中、)?レベルを[＋\+](?<level>\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups["level"].Value;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"gets +{level} level until end of turn"
            }
        ];
    }
}
