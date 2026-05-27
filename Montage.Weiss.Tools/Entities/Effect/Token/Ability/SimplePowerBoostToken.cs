namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches simple power boost clauses for this card.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのパワー＋2000。</c></para>
/// <para><b>Regex:</c> ^このカードのパワー＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power value (e.g., "2000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets +2000 power</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different subject references (このキャラ, 自分など)
/// - Different boost phrasing (パワーを＋, PAWER＋)</para>
/// </remarks>
internal class SimplePowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:このカードの)?パワー(?:を)?[＋\+]([XＸ\d]+)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var powerStr = match.Groups[1].Value.Replace("Ｘ", "X");
        var hasSubject = match.Value.Contains("このカード");
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasSubject ? $"this card gets +{powerStr} power" : $"+{powerStr} power"
            }
        ];
    }
}

