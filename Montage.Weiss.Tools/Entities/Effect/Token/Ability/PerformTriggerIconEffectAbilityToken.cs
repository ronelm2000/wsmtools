namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PerformTriggerIconEffectAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^\[\[(?<icon>[^\]]+?)\]\]の効果を行う(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var icon = match.Groups["icon"].Value;
        var iconName = TriggerIconHelper.GetRawIconName(icon);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"perform the [{iconName}] effect"
            }
        ];
    }
}
