namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOtherTriggerIconGrantToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたのすべての領域のトリガーアイコンが\[\[(.+?)\]\]のCXのトリガーアイコンに\[\[(.+?)\]\]を与える。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var fromIcon = match.Groups[1].Value.Replace(".gif", "");
        var toIcon = match.Groups[2].Value.Replace(".gif", "");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your other CX with [{fromIcon.ToUpperInvariant()}] in the trigger icon in all of your zones get [{toIcon.ToUpperInvariant()}] in the trigger icon"
            }
        ];
    }
}
