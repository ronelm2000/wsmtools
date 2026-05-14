namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllZonesCxTriggerIconGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのすべての領域のCXのトリガーアイコンに\[\[(.+?)\]\]を与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups[1].Value.Replace(".gif", "");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"your CX in all of your zones get [{icon.ToUpperInvariant()}] in the trigger icon"
            }
        ];
    }
}
