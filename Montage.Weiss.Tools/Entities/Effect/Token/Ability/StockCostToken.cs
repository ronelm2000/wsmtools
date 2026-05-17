namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches stock cost notation in parentheses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>(2)</c></para>
/// <para><b>Regex:</c> ^\((\d+)\)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Cost value (e.g., "2")</description></item>
/// </list>
/// <para><b>Output:</b> <c>(2)</c> (preserves original format)</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different cost formats (【コスト2】, コスト：2)
/// - Full-width parentheses (（２）)</para>
/// </remarks>
internal class StockCostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\((\d+)\)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"({cost})"
            }
        ];
    }
}
