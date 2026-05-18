namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches fixed damage dealing clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手に4ダメージを与える。</c> or <c>相手に1ダメージを2回与える。</c></para>
/// <para><b>Regex:</b> ^相手に(?&lt;amount&gt;\d+)ダメージ(?:を(?&lt;times&gt;\d+)回)?を与える</para>
/// <para><b>Output:</b> <c>deal 4 damage to your opponent</c> or <c>deal 1 damage to your opponent 2 times</c></para>
/// </remarks>
internal class DealFixedDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に(?<amount>\d+)ダメージ(?:を(?<times>\d+)回)?を与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var amount = match.Groups["amount"].Value;
        var times = match.Groups["times"].Success ? match.Groups["times"].Value : null;

        var damageText = times != null
            ? $"deal {amount} damage to your opponent {times} times"
            : $"deal {amount} damage to your opponent";

        return
        [
            new CardEffectAbility
            {
                AbilityText = damageText
            }
        ];
    }
}
