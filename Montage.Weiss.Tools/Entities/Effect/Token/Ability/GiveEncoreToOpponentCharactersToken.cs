namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Give Encore ability to all opponent characters" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手のキャラすべてに、『【自】 アンコール ［(2)］』を与える。</c></para>
/// <para><b>Regex:</b> ^相手の(?:すべてのキャラに|キャラすべてに)、『\【自\】\s*アンコール\s*［(.+?)］』を与える(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Encore cost text (e.g., "(2)" or "手札のキャラを1枚控え室に置く")</description></item>
/// </list>
/// <para><b>Output:</b> <c>All of your opponent's characters get "[AUTO] Encore [cost]"</c></para>
/// </remarks>
internal class GiveEncoreToOpponentCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の(?:すべてのキャラに|キャラすべてに)、(?:\s)?『\【自\】\s*アンコール\s*［(.+?)］』を与える");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups[1].Value;
        string costEnglish;
        try
        {
            var costAbilities = registry.EffectListRegistry.GetMatch(costText.AsMemory())(registry);
            costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
        }
        catch (NotImplementedException)
        {
            costEnglish = costText;
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = "All of your opponent's characters get \"[AUTO] Encore [" + costEnglish + "]\""
            }
        ];
    }
}
