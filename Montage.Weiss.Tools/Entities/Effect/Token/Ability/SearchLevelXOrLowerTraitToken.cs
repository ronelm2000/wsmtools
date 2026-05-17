namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Search level X or lower trait character" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベル X 以下の《NIKKE》のキャラを 1 枚選び</c></para>
/// <para><b>Regex:</b> ^レベル X 以下の《(.+?)》のキャラを 1 枚選び (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Search your deck for 1 level X or lower &lt;&lt;NIKKE&gt;&gt; character</c></para>
/// </remarks>
internal class SearchLevelXOrLowerTraitToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^レベルX以下の《(.+?)》のキャラを1枚選び");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"Search your deck for 1 level X or lower <<{trait}>> character"
                }
            ];
        }
        return [];
    }
}
