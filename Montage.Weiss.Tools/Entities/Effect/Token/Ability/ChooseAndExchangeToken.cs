namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a card from level and a card from waiting room, then exchange them" clauses.
/// Supports optional trait qualifier on the waiting-room card.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分のレベル置場のカードと控え室の《NIKKE》のキャラを1枚ずつ選び、入れ替える</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?自分の(?&lt;source1&gt;レベル置場のカード(?:《.+?》)?)と(?:(?:控え室の)?(?&lt;source2&gt;.+?))を(?:\d+)枚ずつ選び、入れ替える</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>source1: Level-zone source (e.g., "レベル置場のカード")</description></item>
///   <item><description>source2: Waiting-room source (e.g., "《NIKKE》のキャラ")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Choose 1 card in your level and 1 <<varies>> character in your waiting room, and exchange them</c></para>
/// </remarks>
internal class ChooseAndExchangeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の(?<source1>レベル置場のカード(?:《.+?》)?)と(?:(?:控え室の)?(?<source2>.+?))を(?:\d+)枚ずつ選び、入れ替える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var source1 = match.Groups["source1"].Value;
        var source2 = match.Groups["source2"].Success ? match.Groups["source2"].Value : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = source2.Contains('《')
                    ? $"Choose 1 card in your level and 1 <<{Regex.Match(source2, @"《(.+?)》").Groups[1].Value}>> character in your waiting room, and exchange them"
                    : "Choose 1 card in your level and 1 card in your waiting room, and exchange them"
            }
        ];
    }
}
