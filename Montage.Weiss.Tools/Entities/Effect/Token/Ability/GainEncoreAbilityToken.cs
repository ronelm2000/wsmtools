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
internal class PowerBoostGainEncoreToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PowerBoostGainEncoreToken>();

    public override Regex Matcher => new(@"^このカードのパワーを[＋\+](?<power>\d+)し、このカードは『\【自\】\s*アンコール\s*［(?<cost>.+?)］』を得る(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups["power"].Value;
        var costText = match.Groups["cost"].Value;

        var costMatch = registry.EffectListRegistry.Match(costText.AsMemory());
        string costEnglish;
        if (costMatch != null)
        {
            var costAbilities = costMatch.Translate(registry);
            costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
            if (costEnglish.Length > 0)
                costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];
        }
        else
        {
            costEnglish = costText;
        }

        Log.Debug("PowerBoostGainEncoreToken: power={Power}, cost='{Cost}' -> '{English}'",
            power, costText, costEnglish);

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and \"[AUTO] Encore [{costEnglish}]\"."
            }
        ];
    }
}

internal class GainEncoreAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは『\【自\】\s*アンコール\s*［(?<cost>.+?)］』を得る(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var costText = match.Groups["cost"].Value;

        var costMatch = registry.EffectListRegistry.Match(costText.AsMemory());
        string costEnglish;
        if (costMatch != null)
        {
            var costAbilities = costMatch.Translate(registry);
            costEnglish = string.Join(", ", costAbilities.Select(a => a.AbilityText));
            if (costEnglish.Length > 0)
                costEnglish = char.ToUpper(costEnglish[0]) + costEnglish[1..];
        }
        else
        {
            costEnglish = costText;
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"This card gets \"[AUTO] Encore [{costEnglish}]\"."
            }
        ];
    }
}
