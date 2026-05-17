namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Gain Encore ability" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは『【自】 アンコール ［手札の《NIKKE》のキャラを1枚控え室に置く］』を得る。</c> or <c>このカードは『【自】 アンコール ［手札のキャラを1枚控え室に置く］』を得る。</c></para>
/// <para><b>Regex:</b> ^このカードは『\【自\】\s*アンコール\s*［(?&lt;cost&gt;.+?)］』を得る(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "cost": The encore cost text (e.g., "手札の《NIKKE》のキャラを1枚控え室に置く" or "手札のキャラを1枚控え室に置く")</description></item>
/// </list>
/// <para><b>Output:</b> <c>This card gets "[AUTO] Encore [cost]"</c></para>
/// </remarks>
internal class GainEncoreAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは『\【自\】\s*アンコール\s*［(?<cost>.+?)］』を得る");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups["cost"].Value;
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
                AbilityText = $"This card gets \"[AUTO] Encore [{costEnglish}]\""
            }
        ];
    }
}
