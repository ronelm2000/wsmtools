namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllZonesTriggerIconGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^すべての領域にあるこのカードはトリガーアイコンに\[\[(.+?)\]\]を得る。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var icon = match.Groups[1].Value.Replace(".gif", "");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card in all of your zones gets [{icon.ToUpperInvariant()}] in the trigger icon."
            }
        ];
    }
}
