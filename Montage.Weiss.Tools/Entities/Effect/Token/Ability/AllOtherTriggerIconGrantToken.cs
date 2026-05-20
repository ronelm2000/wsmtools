namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "all of your other cards/CX with [X] in the trigger icon in all of your zones get [Y] in the trigger icon" clauses.
/// Supports both CX-targeting and card-targeting variants.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたのすべての領域のトリガーアイコンに[[soul.gif]]があるカードのトリガーアイコンに[[shot.gif]]を与える。</c></para>
/// <para><b>Regex:</b> ^他のあなたのすべての領域のトリガーアイコン(?:が|に)\[\[(.+?)\]\](?:のCXの|があるカードの)トリガーアイコンに\[\[(.+?)\]\]を与える(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Source trigger icon (e.g., "soul.gif")</description></item>
///   <item><description>Group 2: Target trigger icon (e.g., "shot.gif")</description></item>
/// </list>
/// <para><b>Output:</b> <c>all of your other cards with [SOUL] in the trigger icon in all of your zones get [SHOT] in the trigger icon</c></para>
/// </remarks>
internal class AllOtherTriggerIconGrantToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたのすべての領域のトリガーアイコン(?:が|に)\[\[(.+?)\]\](?:のCXの|があるカードの)トリガーアイコンに\[\[(.+?)\]\]を与える(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var fromIcon = match.Groups[1].Value.Replace(".gif", "");
        var toIcon = match.Groups[2].Value.Replace(".gif", "");
        var isCards = span.ToString().Contains("カード");
        var target = isCards ? "cards" : "CX";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your other {target} with [{fromIcon.ToUpperInvariant()}] in the trigger icon in all of your zones get [{toIcon.ToUpperInvariant()}] in the trigger icon"
            }
        ];
    }
}
