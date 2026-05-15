namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTriggerCxFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札のトリガーアイコンが\[\[(?<icon>[^\]]+?)\]\]の\s*CX\s*を\s*1\s*枚\s*控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconHelper.GetIconName(icon);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 CX with [{iconName}] in its trigger icon in your hand to your waiting room"
            }
        ];
    }
}
