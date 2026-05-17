namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Per opponent REST character" power boost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の【レスト】しているキャラ 1 枚につき、このカードのパワーを＋1000。</c></para>
/// <para><b>Regex:</b> ^相手の【レスト】しているキャラ 1 枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power value (e.g., "1000")</description></item>
/// </list>
/// <para><b>Output:</b> <c>This card gets +1000 power for each of your opponent's [REST] characters</c></para>
/// </remarks>
internal class PowerBoostPerOpponentRestToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の【レスト】しているキャラ1枚につき、このカードのパワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var power = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"This card gets +{power} power for each of your opponent's [REST] characters"
                }
            ];
        }
        return [];
    }
}
