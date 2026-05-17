namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches soul boost clauses for this card.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのソウルを＋2。</c></para>
/// <para><b>Regex:</c> ^このカードのソウルを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Soul value (e.g., "2")</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets +2 soul</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different subject references (このキャラのソウル)
/// - Different phrasing (ソウル＋, ソウルをプラス)</para>
/// </remarks>
internal class SoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのソウルを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var soul = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{soul} soul"
            }
        ];
    }
}
